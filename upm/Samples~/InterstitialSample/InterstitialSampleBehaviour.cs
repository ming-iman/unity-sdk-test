using UnityEngine;

namespace MSP.Unity.Samples
{
    public sealed class InterstitialSampleBehaviour : MonoBehaviour
    {
        private readonly MSPAdLoader loader = new MSPAdLoader();
        private MSPInterstitialAd cachedAd;
        [SerializeField] private string placementId = "demo-android-interstitial";

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

            loader.LoadAd(placementId, listener, new MSPAdRequest(placementId));
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
