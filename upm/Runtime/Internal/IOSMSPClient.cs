using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MSP.Unity.Adapter;

namespace MSP.Unity.Internal
{
    internal sealed class IOSMSPClient : IMSPClient
    {
        private readonly Dictionary<string, string> placementTokens = new Dictionary<string, string>();

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr msp_unity_get_version();

        [DllImport("__Internal")]
        private static extern void msp_unity_set_log_level(int level);

        [DllImport("__Internal")]
        private static extern void msp_unity_activate_adapter(string adapterId, string bootstrapClassName);

        [DllImport("__Internal")]
        private static extern void msp_unity_initialize(string prebidApiKey, int orgId, int appId, bool isInTestMode);

        [DllImport("__Internal")]
        private static extern void msp_unity_load_ad(string placementId, string requestToken, string adNetwork);

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
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_set_log_level(level);
#else
            _ = level;
#endif
        }

        public void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete)
        {
            MSPUnityListener.SetPendingInitCallback(onComplete);
#if UNITY_IOS && !UNITY_EDITOR
            ActivateIosAdapters();
            msp_unity_initialize(initParams.PrebidApiKey, initParams.OrgId, initParams.AppId, initParams.IsInTestMode);
#else
            onComplete?.Invoke(true, "MSP iOS init called.");
#endif
        }

        public void LoadAd(string placementId, MSPAdRequest adRequest, MSPAdListener adListener)
        {
            var token = $"{placementId}-{Guid.NewGuid():N}";
            placementTokens[placementId] = token;
            MSPUnityListener.RegisterLoadListener(token, placementId, adListener);
#if UNITY_IOS && !UNITY_EDITOR
            var adNetwork = ResolveAdNetwork(adRequest);
            msp_unity_load_ad(placementId, token, adNetwork ?? string.Empty);
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

#if UNITY_IOS && !UNITY_EDITOR
        private static void ActivateIosAdapters()
        {
            MSPUnityAdapterRegistry.EnsureDiscovered();

            foreach (var adapter in MSPUnityAdapterRegistry.GetAll())
            {
                msp_unity_activate_adapter(adapter.AdapterId, adapter.IosBootstrapClassName ?? string.Empty);
            }
        }
#endif

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
