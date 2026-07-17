using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using MSP.Unity;
using MSP.Unity.Adapter;

public enum GridLightMspLoadState
{
    Initializing,
    Loading,
    Ready,
    Failed,
    Showing,
}

public sealed class GridLightMspLoadStatus
{
    public GridLightMspLoadState State { get; }
    public string Seat { get; }
    public string Error { get; }
    public string PlacementId { get; }

    public GridLightMspLoadStatus(
        GridLightMspLoadState state,
        string placementId = "",
        string seat = "",
        string error = "")
    {
        State = state;
        PlacementId = placementId ?? string.Empty;
        Seat = seat ?? string.Empty;
        Error = error ?? string.Empty;
    }

    public string DisplayText
    {
        get
        {
            switch (State)
            {
                case GridLightMspLoadState.Initializing:
                    return "Ad: initializing...";
                case GridLightMspLoadState.Loading:
                    return "Ad: loading...";
                case GridLightMspLoadState.Ready:
                    return string.IsNullOrWhiteSpace(Seat)
                        ? "Ad: ready"
                        : $"Ad: ready · seat {Seat}";
                case GridLightMspLoadState.Failed:
                    return string.IsNullOrWhiteSpace(Error)
                        ? "Ad: failed"
                        : $"Ad: failed · {Error}";
                case GridLightMspLoadState.Showing:
                    return string.IsNullOrWhiteSpace(Seat)
                        ? "Ad: showing"
                        : $"Ad: showing · seat {Seat}";
                default:
                    return "Ad: —";
            }
        }
    }
}

/// <summary>
/// MSP interstitial lifecycle: init + preload on cold start, show on level complete, preload next after dismiss.
/// Installed adapters (nova/google/facebook/liftoff/moloco) register via UPM packages.
/// Force a network via TestParams["ad_network"], e.g. msp_nova / msp_google / msp_facebook / msp_liftoff / msp_moloco.
/// </summary>
public sealed class GridLightMspAds : MonoBehaviour
{
    /// <summary>
    /// Grace period after a load error before starting another request.
    /// Prebid/Facebook bids can arrive after auction timeout; keep the same listener alive.
    /// </summary>
    private const float LoadErrorGraceSeconds = 20f;

    public static GridLightMspAds Instance { get; private set; }

    private readonly MSPAdLoader _loader = new MSPAdLoader();
    private MSPInterstitialAd _cachedAd;
    private MSPAdListener _loadListener;
    private MSPAdRequest _activeRequest;
    private GridLightAppProfile _profile;
    private bool _initialized;
    private bool _loading;
    private bool _adReady;
    private bool _awaitingAdDismiss;
    private bool _pausedForAd;
    private float _showStartedAt;
    private int _loadGeneration;
    private Action _pendingShowComplete;
    private Coroutine _dismissWatchdog;
    private Coroutine _loadErrorGrace;
    private GridLightMspLoadStatus _currentStatus =
        new GridLightMspLoadStatus(GridLightMspLoadState.Initializing);
    private string _lastParsedSeat = string.Empty;
    private bool _logHookActive;

    private static readonly Regex SeatLogRegex = new Regex(
        @"\bseat=([A-Za-z0-9_]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static event Action<GridLightMspLoadStatus> StatusChanged;
    public GridLightMspLoadStatus CurrentStatus => _currentStatus;

    private string PlacementId => GridLightAppProfiles.CurrentPlacement;
    private string SelectedAdNetwork => GridLightAppProfiles.CurrentAdNetwork;
    private string SelectedTestAdNetwork => GridLightAppProfiles.CurrentTestAdNetwork;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("GridLightMspAds");
        go.AddComponent<GridLightMspAds>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _profile = GridLightAppProfiles.Current;
        MSP.Unity.MSP.SetLogLevel(MSPLogLevel.VERBOSE);
        EnsureLoadListener();
        PublishStatus(new GridLightMspLoadStatus(GridLightMspLoadState.Initializing, PlacementId));
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SetLogHookActive(false);
    }

    private void Start()
    {
        LogRegisteredAdapters();
        var initParams = _profile.ToMspInitializationParameters();
        Debug.Log(
            $"[GridLight MSP] Initializing profile={_profile.DisplayName} " +
            $"apiKey={initParams.PrebidApiKey} orgId={initParams.OrgId} appId={initParams.AppId} " +
            $"host={initParams.PrebidHost} placement={PlacementId} adNetwork={SelectedAdNetwork}");
        MSP.Unity.MSP.Initialize(initParams, (success, message) =>
        {
            _initialized = success;
            Debug.Log($"[GridLight MSP] Init complete. success={success} message={message}");
            if (success)
            {
                LoadNextAd();
                return;
            }

            PublishStatus(new GridLightMspLoadStatus(
                GridLightMspLoadState.Failed,
                PlacementId,
                error: string.IsNullOrWhiteSpace(message) ? "init failed" : message));
        });
    }

    private static void LogRegisteredAdapters()
    {
        var adapters = MSPUnityAdapterRegistry.GetAll();
        if (adapters == null || adapters.Count == 0)
        {
            Debug.LogWarning("[GridLight MSP] No adapters registered yet.");
            return;
        }

        var sb = new StringBuilder("[GridLight MSP] Registered adapters:");
        foreach (var adapter in adapters)
            sb.Append(' ').Append(adapter.AdapterId);
        Debug.Log(sb.ToString());
    }

    public void TryShowInterstitial(Action onComplete)
    {
        if (_adReady && _cachedAd != null)
        {
            _pendingShowComplete = onComplete;
            _awaitingAdDismiss = true;
            _pausedForAd = false;
            _showStartedAt = Time.unscaledTime;
            _adReady = false;
            Debug.Log($"[GridLight MSP] Showing interstitial: {PlacementId}");
            PublishStatus(new GridLightMspLoadStatus(
                GridLightMspLoadState.Showing,
                PlacementId,
                _currentStatus.Seat));
            _cachedAd.Show();
            if (_dismissWatchdog != null) StopCoroutine(_dismissWatchdog);
            _dismissWatchdog = StartCoroutine(AdDismissWatchdog());
            return;
        }

        Debug.LogWarning("[GridLight MSP] No interstitial ready; skipping show.");
        PublishStatus(new GridLightMspLoadStatus(
            GridLightMspLoadState.Failed,
            PlacementId,
            error: "not ready"));
        onComplete?.Invoke();
        if (_initialized && !_loading) LoadNextAd();
    }

    private void OnApplicationPause(bool paused)
    {
        if (!_awaitingAdDismiss) return;

        if (paused)
        {
            _pausedForAd = true;
            return;
        }

        if (_pausedForAd)
            FinishAdFlow("application-pause");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!_awaitingAdDismiss || !hasFocus) return;
        if (_pausedForAd || Time.unscaledTime - _showStartedAt > 0.75f)
            FinishAdFlow("application-focus");
    }

    private IEnumerator AdDismissWatchdog()
    {
        yield return new WaitForSecondsRealtime(180f);
        if (_awaitingAdDismiss)
            FinishAdFlow("watchdog-timeout");
    }

    private void FinishAdFlow(string reason)
    {
        if (!_awaitingAdDismiss) return;

        Debug.Log($"[GridLight MSP] Ad flow finished ({reason}).");
        _awaitingAdDismiss = false;
        _pausedForAd = false;
        _adReady = false;
        _cachedAd = null;

        if (_dismissWatchdog != null)
        {
            StopCoroutine(_dismissWatchdog);
            _dismissWatchdog = null;
        }

        var callback = _pendingShowComplete;
        _pendingShowComplete = null;
        callback?.Invoke();

        if (_initialized && !_loading) LoadNextAd();
    }

    private void EnsureLoadListener()
    {
        if (_loadListener != null) return;

        _loadListener = new MSPAdListener
        {
            OnAdLoaded = HandleAdLoaded,
            OnError = HandleLoadError,
            OnAdDismissed = _ => FinishAdFlow("sdk-dismiss"),
            OnAdImpression = HandleAdImpression,
            OnAdClick = HandleAdClick,
        };
    }

    private void HandleAdImpression(MSPAd mspAd)
    {
        Debug.Log($"Ad Impression: mspAd = {mspAd != null}");
    }

    private void HandleAdClick(MSPAd mspAd)
    {
        Debug.Log($"Ad Click: mspAd = {mspAd != null}");
    }

    private void HandleAdLoaded(string placementId, Dictionary<string, object> loadInfo)
    {
        if (_loadErrorGrace != null)
        {
            StopCoroutine(_loadErrorGrace);
            _loadErrorGrace = null;
        }

        _loading = false;
        SetLogHookActive(false);
        _cachedAd = _loader.GetAd(placementId) as MSPInterstitialAd;
        _adReady = _cachedAd != null;
        var seat = ResolveSeat(loadInfo);
        Debug.Log($"[GridLight MSP] Ad loaded: {placementId} ready={_adReady} seat={seat} generation={_loadGeneration}");
        PublishStatus(new GridLightMspLoadStatus(
            _adReady ? GridLightMspLoadState.Ready : GridLightMspLoadState.Failed,
            placementId,
            seat,
            _adReady ? string.Empty : "cache miss"));
    }

    private void HandleLoadError(string message, Dictionary<string, object> loadInfo)
    {
        Debug.LogError(
            $"[GridLight MSP] Load error: {message} generation={_loadGeneration}. " +
            "Keeping listener alive for late bid callbacks.");

        var seat = ResolveSeat(loadInfo);
        if (!string.IsNullOrWhiteSpace(seat))
            _lastParsedSeat = seat;

        PublishStatus(new GridLightMspLoadStatus(
            GridLightMspLoadState.Loading,
            PlacementId,
            seat,
            "waiting for late bid"));

        if (_loadErrorGrace != null) StopCoroutine(_loadErrorGrace);
        _loadErrorGrace = StartCoroutine(ReleaseLoadAfterGracePeriod(_loadGeneration));
    }

    private IEnumerator ReleaseLoadAfterGracePeriod(int generation)
    {
        yield return new WaitForSecondsRealtime(LoadErrorGraceSeconds);
        if (generation != _loadGeneration || _adReady)
        {
            _loadErrorGrace = null;
            yield break;
        }

        _loading = false;
        _loadErrorGrace = null;
        SetLogHookActive(false);
        PublishStatus(new GridLightMspLoadStatus(
            GridLightMspLoadState.Failed,
            PlacementId,
            _lastParsedSeat,
            "load timeout"));
        Debug.LogWarning(
            $"[GridLight MSP] Load grace period expired for generation={generation}; ready for retry.");
    }

    private void LoadNextAd()
    {
        if (!_initialized || _loading) return;

        if (_loadErrorGrace != null)
        {
            StopCoroutine(_loadErrorGrace);
            _loadErrorGrace = null;
        }

        _loading = true;
        _adReady = false;
        _cachedAd = null;
        _lastParsedSeat = string.Empty;
        _loadGeneration++;
        EnsureLoadListener();
        SetLogHookActive(true);

        _activeRequest = CreateAdRequest();
        Debug.Log($"[GridLight MSP] Loading interstitial: {PlacementId} generation={_loadGeneration}");
        PublishStatus(new GridLightMspLoadStatus(GridLightMspLoadState.Loading, PlacementId));
        _loader.LoadAd(PlacementId, _loadListener, _activeRequest);
    }

    private MSPAdRequest CreateAdRequest()
    {
        var request = new MSPAdRequest(PlacementId);
        var testAdNetwork = SelectedTestAdNetwork;
        if (!string.IsNullOrWhiteSpace(testAdNetwork))
        {
            request.TestParams["test_ad"] = true;
            request.TestParams["ad_network"] = testAdNetwork;
        }
        return request;
    }

    private void PublishStatus(GridLightMspLoadStatus status)
    {
        _currentStatus = status;
        StatusChanged?.Invoke(status);
    }

    private string ResolveSeat(Dictionary<string, object> loadInfo)
    {
        var seat = ExtractSeat(loadInfo);
        if (!string.IsNullOrWhiteSpace(seat))
        {
            _lastParsedSeat = seat;
            return seat;
        }

        if (!string.IsNullOrWhiteSpace(_lastParsedSeat))
            return _lastParsedSeat;

        var forcedNetwork = SelectedTestAdNetwork;
        return string.IsNullOrWhiteSpace(forcedNetwork) ? string.Empty : forcedNetwork;
    }

    private static string ExtractSeat(Dictionary<string, object> loadInfo)
    {
        if (loadInfo == null || loadInfo.Count == 0) return string.Empty;

        string[] keys =
        {
            "seat",
            "ad_network",
            "adNetwork",
            "bidder",
            "bidderName",
            "network",
            "client_bidder",
            "winning_seat",
        };

        foreach (var key in keys)
        {
            if (!loadInfo.TryGetValue(key, out var value) || value == null) continue;
            var text = value.ToString();
            if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
        }

        return string.Empty;
    }

    private void SetLogHookActive(bool active)
    {
        if (active)
        {
            if (_logHookActive) return;
            Application.logMessageReceived += OnUnityLogMessage;
            _logHookActive = true;
            return;
        }

        if (!_logHookActive) return;
        Application.logMessageReceived -= OnUnityLogMessage;
        _logHookActive = false;
    }

    private void OnUnityLogMessage(string condition, string stackTrace, LogType type)
    {
        if (!_loading && _loadErrorGrace == null) return;
        if (type != LogType.Log && type != LogType.Warning) return;

        var match = SeatLogRegex.Match(condition);
        if (!match.Success) return;

        var seat = match.Groups[1].Value;
        if (string.IsNullOrWhiteSpace(seat)) return;

        _lastParsedSeat = seat;
        PublishStatus(new GridLightMspLoadStatus(
            GridLightMspLoadState.Loading,
            PlacementId,
            seat,
            _loadErrorGrace != null ? "waiting for late bid" : string.Empty));
    }
}
