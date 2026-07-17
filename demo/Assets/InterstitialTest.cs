using UnityEngine;
using MSP.Unity;

public class InterstitialTest : MonoBehaviour
{
    // Demo / test credentials — replace with your MSP account values before production use.
    private const string DemoApiKey = "af7ce3f9-462d-4df1-815f-09314bb87ca3";
    private const int DemoOrgId = 1061;
    private const int DemoAppId = 1;

    private static string DefaultPlacementId
    {
        get
        {
#if UNITY_IOS
            return "demo-ios-launch-fullscreen";
#else
            return "demo-android-interstitial";
#endif
        }
    }

    private MSPAdLoader loader;
    private MSPInterstitialAd cachedAd;
    [SerializeField] private string placementId = DefaultPlacementId;
    [SerializeField] private string testAdNetwork = "msp_nova";
    [SerializeField] private bool forceTestAd = true;

    private bool isInitialized;

    private void Awake()
    {
        MSP.Unity.MSP.SetLogLevel(MSPLogLevel.VERBOSE);
#if UNITY_IOS
        // Force iOS runtime values to avoid stale serialized Scene data.
        placementId = "demo-ios-launch-fullscreen";
        testAdNetwork = "msp_nova";
#endif
    }

    void Start()
    {
        Debug.Log("MSP start to init...");
        MSP.Unity.MSP.Initialize(new MSPInitializationParameters
        {
            PrebidApiKey = DemoApiKey,
            OrgId = DemoOrgId,
            AppId = DemoAppId,
            IsInTestMode = true
        }, (success, message) =>
        {
            isInitialized = success;
            Debug.Log($"MSP init complete. success={success} message={message}");
        });
    }

    public void Load()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("MSP not initialized yet. Wait for init callback.");
            return;
        }

        loader?.Dispose();
        loader = new MSPAdLoader();
        cachedAd = null;

        var currentLoader = loader;
        var listener = new MSPAdListener
        {
            OnAdLoaded = (pid, _) =>
            {
                cachedAd = currentLoader.GetAd(pid) as MSPInterstitialAd;
                Debug.Log($"Loaded: {pid}");
            },
            OnError = (msg, _) => Debug.LogError($"Load error: {msg}"),
            OnAdDismissed = _ => Debug.Log("Dismissed")
        };

        var request = new MSPAdRequest(placementId);
        if (forceTestAd)
        {
            request.TestParams["test_ad"] = true;
        }

        if (!string.IsNullOrWhiteSpace(testAdNetwork))
        {
            request.TestParams["ad_network"] = testAdNetwork;
        }

        currentLoader.LoadAd(placementId, listener, request);
    }

    public void Show()
    {
        if (cachedAd == null)
        {
            Debug.LogWarning("No ad yet. Load first.");
            return;
        }
        cachedAd.Show();
    }

    private void OnDestroy()
    {
        loader?.Dispose();
        loader = null;
    }

    private void OnGUI()
    {
        const int width = 320;
        const int height = 96;
        const int spacing = 24;

        var totalHeight = height * 2 + spacing;
        var left = (Screen.width - width) / 2;
        var top = (Screen.height - totalHeight) / 2;

        if (GUI.Button(new Rect(left, top, width, height), "Load"))
        {
            Load();
        }

        if (GUI.Button(new Rect(left, top + height + spacing, width, height), "Show"))
        {
            Show();
        }
    }
}
