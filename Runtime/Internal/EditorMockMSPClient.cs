using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSP.Unity.Internal
{
    internal sealed class EditorMockMSPClient : IMSPClient
    {
        private readonly Dictionary<string, string> placementTokens = new Dictionary<string, string>();

        public string Version => "mock-0.1.0";

        public void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete)
        {
            if (onComplete != null)
            {
                onComplete(true, "MSP mock initialized.");
            }
        }

        public void LoadAd(string placementId, MSPAdRequest adRequest, MSPAdListener adListener)
        {
            var token = $"mock-{placementId}-{Guid.NewGuid():N}";
            placementTokens[placementId] = token;

            var loadInfo = new Dictionary<string, object>
            {
                { "adNetwork", "mock" },
                { "placementId", placementId },
                { "requestId", token }
            };
            if (adListener != null && adListener.OnAdLoaded != null)
            {
                adListener.OnAdLoaded(placementId, loadInfo);
            }
        }

        public MSPAd GetAd(string placementId, MSPAdListener adListener)
        {
            if (!placementTokens.TryGetValue(placementId, out var token))
            {
                return null;
            }

            var ad = new MSPInterstitialAd(placementId, this, adListener)
            {
                NativeAdToken = token
            };
            return ad;
        }

        public void ShowAd(string placementId, string nativeAdToken)
        {
            Debug.Log($"[MSP Mock] Show interstitial. placementId={placementId}, token={nativeAdToken}");
        }
    }
}
