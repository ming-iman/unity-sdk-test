using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MSP.Unity.Internal
{
    internal sealed class IOSMSPClient : IMSPClient
    {
        private readonly Dictionary<string, string> placementTokens = new Dictionary<string, string>();

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr msp_unity_get_version();

        [DllImport("__Internal")]
        private static extern void msp_unity_initialize(string prebidApiKey, int orgId, int appId, bool isInTestMode);

        [DllImport("__Internal")]
        private static extern void msp_unity_load_ad(string placementId, string requestToken);

        [DllImport("__Internal")]
        private static extern bool msp_unity_get_ad(string placementId, string requestToken);

        [DllImport("__Internal")]
        private static extern void msp_unity_show_ad(string placementId, string requestToken);
#endif

        public string Version
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return Marshal.PtrToStringAnsi(msp_unity_get_version()) ?? "unknown";
#else
                return "ios-editor-stub";
#endif
            }
        }

        public void SetLogLevel(int level)
        {
            // TODO: expose iOS MSPLogger bridge if needed.
            _ = level;
        }

        public void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete)
        {
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_initialize(initParams.PrebidApiKey, initParams.OrgId, initParams.AppId, initParams.IsInTestMode);
#endif
            if (onComplete != null)
            {
                onComplete(true, "MSP iOS init called.");
            }
        }

        public void LoadAd(string placementId, MSPAdRequest adRequest, MSPAdListener adListener)
        {
            var token = $"{placementId}-{Guid.NewGuid():N}";
            placementTokens[placementId] = token;
            MSPUnityListener.RegisterLoadListener(token, placementId, adListener);
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_load_ad(placementId, token);
#endif
        }

        public MSPAd GetAd(string placementId, MSPAdListener adListener)
        {
            if (!placementTokens.TryGetValue(placementId, out var nativeAdToken))
            {
                return null;
            }

#if UNITY_IOS && !UNITY_EDITOR
            if (!msp_unity_get_ad(placementId, nativeAdToken))
            {
                return null;
            }
#else
            if (string.IsNullOrEmpty(nativeAdToken))
            {
                return null;
            }
#endif
            var ad = new MSPInterstitialAd(placementId, this, adListener)
            {
                NativeAdToken = nativeAdToken
            };
            return ad;
        }

        public void ShowAd(string placementId, string nativeAdToken)
        {
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_show_ad(placementId, nativeAdToken);
#endif
        }
    }
}
