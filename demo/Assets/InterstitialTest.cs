using UnityEngine;
using MSP.Unity;

public class InterstitialTest : MonoBehaviour
{
    // Copied from msp-android demo app (AppProfiles.DEMO_APP).
    private const string DemoApiKey = "af7ce3f9-462d-4df1-815f-09314bb87ca3";
    private const int DemoOrgId = 1061;
    private const int DemoAppId = 1;

    private MSPAdLoader loader = new MSPAdLoader();
    private MSPInterstitialAd cachedAd;
    [SerializeField] private string placementId = "demo-android-interstitial";
    [SerializeField] private string adNetwork = "msp_nova";

    private bool isInitialized;

    void Start()
    {
        Debug.Log("MSP start to init...");
        MSP.Unity.MSP.SetLogLevel(MSPLogLevel.VERBOSE);
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
        var listener = new MSPAdListener
        {
            OnAdLoaded = (pid, _) =>
            {
                cachedAd = loader.GetAd(pid) as MSPInterstitialAd;
                Debug.Log($"Loaded: {pid}");
            },
            OnError = (msg, _) => Debug.LogError($"Load error: {msg}"),
            OnAdDismissed = _ => Debug.Log("Dismissed")
        };

        var request = new MSPAdRequest(placementId, adNetwork);
        loader.LoadAd(placementId, listener, request);
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

    private void OnGUI()
    {
        const int width = 220;
        const int height = 60;
        const int left = 20;
        const int top = 20;
        const int spacing = 16;

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