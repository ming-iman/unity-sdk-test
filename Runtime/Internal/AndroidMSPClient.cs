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

        public void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete)
        {
            // Bridge signature to be implemented in Android plugin module.
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("initialize", initParams.PrebidApiKey, initParams.OrgId, initParams.AppId, initParams.IsInTestMode);
            if (onComplete != null)
            {
                onComplete(true, "MSP Android init called.");
            }
        }

        public void LoadInterstitial(string placementId, MSPAdRequest adRequest, MSPAdListener adListener, Action<string> cacheAdToken)
        {
            var token = $"{placementId}-{Guid.NewGuid():N}";
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("loadInterstitial", placementId, token);
            cacheAdToken(token);
        }

        public void ShowInterstitial(string placementId, string nativeAdToken)
        {
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("showInterstitial", placementId, nativeAdToken);
        }
    }
}
