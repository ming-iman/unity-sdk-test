using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSP.Unity.Internal
{
    internal sealed class EditorMockMSPClient : IMSPClient
    {
        public string Version => "mock-0.1.0";

        public void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete)
        {
            if (onComplete != null)
            {
                onComplete(true, "MSP mock initialized.");
            }
        }

        public void LoadInterstitial(string placementId, MSPAdRequest adRequest, MSPAdListener adListener, Action<string> cacheAdToken)
        {
            var token = $"mock-{placementId}-{Guid.NewGuid():N}";
            cacheAdToken(token);

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

        public void ShowInterstitial(string placementId, string nativeAdToken)
        {
            Debug.Log($"[MSP Mock] Show interstitial. placementId={placementId}, token={nativeAdToken}");
        }
    }
}
