using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSP.Unity.Internal
{
    internal sealed class EditorMockMSPClient : IMSPClient
    {
        private readonly HashSet<string> loaderIds = new HashSet<string>();
        private readonly Dictionary<string, string> loadedPlacementByLoader = new Dictionary<string, string>();
        private int logLevel = MSPLogLevel.NONE;

        public string Version => "mock-0.1.0";

        public void SetLogLevel(int level)
        {
            logLevel = level;
            Debug.Log($"[MSP Mock] Log level set to {logLevel}");
        }

        public void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete)
        {
            if (onComplete != null)
            {
                onComplete(true, "MSP mock initialized.");
            }
        }

        public string CreateAdLoader()
        {
            var loaderId = Guid.NewGuid().ToString("N");
            loaderIds.Add(loaderId);
            return loaderId;
        }

        public void DestroyAdLoader(string loaderId)
        {
            loaderIds.Remove(loaderId);
            loadedPlacementByLoader.Remove(loaderId);
        }

        public void LoadAd(string loaderId, string placementId, MSPAdRequest adRequest, MSPAdListener adListener)
        {
            if (!loaderIds.Contains(loaderId))
            {
                adListener?.OnError?.Invoke("Unknown MSPAdLoader", new Dictionary<string, object>());
                return;
            }

            loadedPlacementByLoader[loaderId] = placementId;
            MSPUnityListener.RegisterLoadListener(loaderId, adListener);

            var loadInfo = new Dictionary<string, object>
            {
                { "adNetwork", "mock" },
                { "placementId", placementId },
                { "loaderId", loaderId }
            };
            adListener?.OnAdLoaded?.Invoke(placementId, loadInfo);
        }

        public MSPAd GetAd(string loaderId, string placementId, MSPAdListener adListener)
        {
            if (!loadedPlacementByLoader.TryGetValue(loaderId, out var loadedPlacement) ||
                loadedPlacement != placementId)
            {
                return null;
            }

            return new MSPInterstitialAd(placementId, this, adListener)
            {
                LoaderId = loaderId
            };
        }

        public void ShowAd(string loaderId)
        {
            Debug.Log($"[MSP Mock] Show interstitial. loaderId={loaderId}");
        }
    }
}
