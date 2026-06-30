using System;
using System.Runtime.InteropServices;

namespace MSP.Unity.Internal
{
    internal sealed class IOSMSPClient : IMSPClient
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr msp_unity_get_version();

        [DllImport("__Internal")]
        private static extern void msp_unity_initialize(string prebidApiKey, int orgId, int appId, bool isInTestMode);

        [DllImport("__Internal")]
        private static extern void msp_unity_load_interstitial(string placementId, string requestToken);

        [DllImport("__Internal")]
        private static extern void msp_unity_show_interstitial(string placementId, string requestToken);
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

        public void LoadInterstitial(string placementId, MSPAdRequest adRequest, MSPAdListener adListener, Action<string> cacheAdToken)
        {
            var token = $"{placementId}-{Guid.NewGuid():N}";
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_load_interstitial(placementId, token);
#endif
            cacheAdToken(token);
        }

        public void ShowInterstitial(string placementId, string nativeAdToken)
        {
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_show_interstitial(placementId, nativeAdToken);
#endif
        }
    }
}
