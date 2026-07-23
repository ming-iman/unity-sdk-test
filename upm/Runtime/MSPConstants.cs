namespace MSP.Unity
{
    /// <summary>
    /// Well-known MSP parameter keys for Unity callers.
    /// iOS init keys match native <c>InitializationParametersCustomKeys</c>;
    /// Android init keys match native <c>MSPConstants</c> INIT_PARAM_* values.
    /// Shared custom / load-info / user-signal keys match both native MSPConstants where the wire value is the same.
    /// </summary>
    public static class MSPConstants
    {
        public static class Ios
        {
            public const string InitParamKeyUnityAppKey = "unityAppKey";
            public const string InitParamKeyInmobiAccountId = "inmobiAccountId";
            public const string InitParamKeyMintegralAppId = "mintegralAppId";
            public const string InitParamKeyMintegralApiKey = "mintegralApiKey";
            public const string InitParamKeyPubmaticPublisherId = "pubmaticPublisherId";
            public const string InitParamKeyPubmaticProfileIds = "pubmaticProfileIds";
            public const string InitParamKeyPubmaticStoreUrl = "pubmaticStoreUrl";
            public const string InitParamKeyAmazonAppKey = "amazonAppKey";
            public const string InitParamKeyMolocoAppKey = "molocoAppKey";
            public const string InitParamKeyLiftoffAppId = "liftoffAppId";
            public const string InitParamKeyApplovinSdkKey = "applovinSdkKey";
            public const string InitParamKeyPrebidBidRequestTimeoutMillis = "prebidBidRequestTimeoutMillis";
            public const string InitParamKeyPrebidBannerVastFix = "prebidBannerVastFix";
        }

        public static class Android
        {
            public const string InitParamKeyPpid = "ppid";
            public const string InitParamKeyEmail = "email";
            public const string InitParamKeyUnityAppKey = "unity_app_key";
            public const string InitParamKeyInmobiAccountId = "inmobi_account_id";
            public const string InitParamKeyMintegralAppId = "mintegral_app_id";
            public const string InitParamKeyMintegralAppKey = "mintegral_app_key";
            public const string InitParamKeyPubmaticPublisherId = "pubmatic_publisher_id";
            public const string InitParamKeyPubmaticProfileIds = "pubmatic_profile_ids";
            public const string InitParamKeyMolocoAppKey = "moloco_app_key";
            public const string InitParamKeyAmazonAppKey = "amazon_app_key";
            public const string InitParamKeyLiftoffAppId = "liftoff_app_id";
            public const string InitParamKeyGoogleAppId = "google_app_id";
            public const string InitParamKeyApplovinSdkKey = "applovin_sdk_key";
            public const string InitParamKeyPrebidBidRequestTimeoutMillis = "prebid_bid_request_timeout_millis";
        }

        public const string CustomParamKeyUserId = "user_id";
        public const string CustomParamKeyDebugItem = "debug_item";
        public const string CustomParamKeyTest = "test";

        public const string LoadInfoKeyRequestId = "request_id";

        public const string UserSignalAppInstallTime = "app_install_time";
        public const string UserSignalIsFirstInstall = "is_first_install";
        public const string UserSignalPpid = "ppid";
    }
}
