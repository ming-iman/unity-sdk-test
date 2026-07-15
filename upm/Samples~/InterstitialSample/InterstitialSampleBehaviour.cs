using UnityEngine;

namespace MSP.Unity.Samples
{
    public sealed class InterstitialSampleBehaviour : MonoBehaviour
    {
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

        private readonly MSPAdLoader loader = new MSPAdLoader();
        private MSPInterstitialAd cachedAd;
        [SerializeField] private string placementId = DefaultPlacementId;
        [SerializeField] private string testAdNetwork = "msp_nova";
        [SerializeField] private bool forceTestAd = true;

        private void Awake()
        {
            MSP.SetLogLevel(MSPLogLevel.VERBOSE);
#if UNITY_IOS
            // Force iOS runtime values to avoid stale serialized Scene/Prefab data.
            placementId = "demo-ios-launch-fullscreen";
            testAdNetwork = "msp_nova";
#endif
        }

        private void Start()
        {
            MSP.Initialize(new MSPInitializationParameters
            {
                PrebidApiKey = "demo-api-key",
                OrgId = 1,
                AppId = 1,
                IsInTestMode = true
            });
        }

        public void Load()
        {
            var listener = new MSPAdListener
            {
                OnAdLoaded = (pid, _) =>
                {
                    cachedAd = loader.GetAd(pid) as MSPInterstitialAd;
                    Debug.Log($"[MSP Sample] Ad loaded for {pid}");
                },
                OnError = (message, _) => Debug.LogError($"[MSP Sample] Load error: {message}"),
                OnAdDismissed = _ => Debug.Log("[MSP Sample] Interstitial dismissed")
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

            loader.LoadAd(placementId, listener, request);
        }

        public void Show()
        {
            if (cachedAd == null)
            {
                Debug.LogWarning("[MSP Sample] No cached interstitial. Call Load first.");
                return;
            }
            cachedAd.Show();
        }
    }
}
