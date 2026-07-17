using System;
using System.Runtime.InteropServices;
using MSP.Unity.Adapter;

namespace MSP.Unity.Internal
{
    internal sealed class IOSMSPClient : IMSPClient
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr msp_unity_get_version();

        [DllImport("__Internal")]
        private static extern void msp_unity_set_log_level(int level);

        [DllImport("__Internal")]
        private static extern void msp_unity_activate_adapter(string adapterId, string bootstrapClassName);

        [DllImport("__Internal")]
        private static extern void msp_unity_initialize_json(string initializationJson);

        [DllImport("__Internal")]
        private static extern IntPtr msp_unity_create_ad_loader();

        [DllImport("__Internal")]
        private static extern void msp_unity_destroy_ad_loader(string loaderId);

        [DllImport("__Internal")]
        private static extern void msp_unity_load_ad(
            string loaderId,
            string placementId,
            string customParamsJson,
            string testParamsJson);

        [DllImport("__Internal")]
        private static extern bool msp_unity_get_ad(string loaderId, string placementId);

        [DllImport("__Internal")]
        private static extern void msp_unity_show_ad(string loaderId);
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
            msp_unity_initialize_json(MSPParamsJson.Serialize(initParams));
#else
            onComplete?.Invoke(true, "MSP iOS init called.");
#endif
        }

        public string CreateAdLoader()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return Marshal.PtrToStringAnsi(msp_unity_create_ad_loader()) ?? string.Empty;
#else
            return Guid.NewGuid().ToString("N");
#endif
        }

        public void DestroyAdLoader(string loaderId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_destroy_ad_loader(loaderId);
#else
            _ = loaderId;
#endif
        }

        public void LoadAd(string loaderId, string placementId, MSPAdRequest adRequest, MSPAdListener adListener)
        {
            MSPUnityListener.RegisterLoadListener(loaderId, adListener);
#if UNITY_IOS && !UNITY_EDITOR
            var customJson = MSPParamsJson.Serialize(adRequest?.CustomParams);
            var testJson = MSPParamsJson.Serialize(adRequest?.TestParams);
            msp_unity_load_ad(loaderId, placementId, customJson, testJson);
#else
            _ = (loaderId, placementId, adRequest);
#endif
        }

        public MSPAd GetAd(string loaderId, string placementId, MSPAdListener adListener)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!msp_unity_get_ad(loaderId, placementId))
            {
                return null;
            }
#else
            if (string.IsNullOrEmpty(loaderId))
            {
                return null;
            }
#endif
            return new MSPInterstitialAd(placementId, this, adListener)
            {
                LoaderId = loaderId
            };
        }

        public void ShowAd(string loaderId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            msp_unity_show_ad(loaderId);
#else
            _ = loaderId;
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
    }
}
