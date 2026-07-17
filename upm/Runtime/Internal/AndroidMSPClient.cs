using System;
using UnityEngine;

namespace MSP.Unity.Internal
{
    internal sealed class AndroidMSPClient : IMSPClient
    {
        private const string BridgeClassName = "com.particles.msp.unity.MSPUnityBridge";

        public string Version
        {
            get
            {
                using var bridge = new AndroidJavaClass(BridgeClassName);
                return bridge.CallStatic<string>("getVersion");
            }
        }

        public void SetLogLevel(int level)
        {
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("setLogLevel", level);
        }

        public void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete)
        {
            MSPUnityListener.SetPendingInitCallback(onComplete);
            var initializationJson = MSPParamsJson.Serialize(initParams);
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("initializeJson", initializationJson);
        }

        public string CreateAdLoader()
        {
            using var bridge = new AndroidJavaClass(BridgeClassName);
            return bridge.CallStatic<string>("createAdLoader");
        }

        public void DestroyAdLoader(string loaderId)
        {
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("destroyAdLoader", loaderId);
        }

        public void LoadAd(string loaderId, string placementId, MSPAdRequest adRequest, MSPAdListener adListener)
        {
            MSPUnityListener.RegisterLoadListener(loaderId, adListener);
            var customJson = MSPParamsJson.Serialize(adRequest?.CustomParams);
            var testJson = MSPParamsJson.Serialize(adRequest?.TestParams);
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("loadAd", loaderId, placementId, customJson, testJson);
        }

        public MSPAd GetAd(string loaderId, string placementId, MSPAdListener adListener)
        {
            using var bridge = new AndroidJavaClass(BridgeClassName);
            var found = bridge.CallStatic<bool>("getAd", loaderId, placementId);
            if (!found)
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
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("showAd", loaderId);
        }
    }
}
