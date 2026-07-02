using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSP.Unity.Internal
{
    internal sealed class AndroidMSPClient : IMSPClient
    {
        private const string BridgeClassName = "com.particles.msp.unity.MSPUnityBridge";
        private readonly Dictionary<string, string> placementTokens = new Dictionary<string, string>();

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
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("initialize", initParams.PrebidApiKey, initParams.OrgId, initParams.AppId, initParams.IsInTestMode);
        }

        public void LoadAd(string placementId, MSPAdRequest adRequest, MSPAdListener adListener)
        {
            var token = $"{placementId}-{Guid.NewGuid():N}";
            placementTokens[placementId] = token;
            MSPUnityListener.RegisterLoadListener(token, placementId, adListener);
            var adNetwork = ResolveAdNetwork(adRequest);
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("loadAd", placementId, token, adNetwork);
        }

        public MSPAd GetAd(string placementId, MSPAdListener adListener)
        {
            if (!placementTokens.TryGetValue(placementId, out var nativeAdToken))
            {
                return null;
            }

            using var bridge = new AndroidJavaClass(BridgeClassName);
            var found = bridge.CallStatic<bool>("getAd", placementId, nativeAdToken);
            if (!found)
            {
                return null;
            }

            var ad = new MSPInterstitialAd(placementId, this, adListener)
            {
                NativeAdToken = nativeAdToken
            };
            return ad;
        }

        public void ShowAd(string placementId, string nativeAdToken)
        {
            using var bridge = new AndroidJavaClass(BridgeClassName);
            bridge.CallStatic("showAd", placementId, nativeAdToken);
        }

        private static string ResolveAdNetwork(MSPAdRequest adRequest)
        {
            if (adRequest == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(adRequest.AdNetwork))
            {
                return adRequest.AdNetwork;
            }

            if (adRequest.CustomParams != null &&
                adRequest.CustomParams.TryGetValue("ad_network", out var value) &&
                value != null)
            {
                return value.ToString();
            }

            return null;
        }
    }
}
