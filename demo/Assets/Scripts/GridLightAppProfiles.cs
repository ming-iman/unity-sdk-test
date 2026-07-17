using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MSP.Unity;
using UnityEngine;

public sealed class GridLightAppProfile
{
    public string Id { get; }
    public string DisplayName { get; }
    public string PrebidApiKey { get; }
    public string AndroidPrebidApiKey { get; }
    public string SourceApp { get; }
    public long OrgId { get; }
    public long AppId { get; }
    public string IosPrebidHost { get; }
    public string AndroidPrebidHost { get; }
    public bool IsInTestMode { get; }
    public string AppPackageName { get; }
    public string AppVersionName { get; }
    public IReadOnlyDictionary<string, object> IosParameters { get; }
    public IReadOnlyDictionary<string, object> AndroidParameters { get; }
    public string IosInterstitialPlacement { get; }
    public string AndroidInterstitialPlacement { get; }
    public string IosTestAdNetwork { get; }
    public string AndroidTestAdNetwork { get; }

    public GridLightAppProfile(
        string id,
        string displayName,
        string prebidApiKey,
        string androidPrebidApiKey,
        string sourceApp,
        long orgId,
        long appId,
        string iosPrebidHost,
        string androidPrebidHost,
        bool isInTestMode,
        string appPackageName,
        string appVersionName,
        IReadOnlyDictionary<string, object> iosParameters,
        IReadOnlyDictionary<string, object> androidParameters,
        string iosInterstitialPlacement,
        string androidInterstitialPlacement,
        string iosTestAdNetwork = null,
        string androidTestAdNetwork = null)
    {
        Id = id;
        DisplayName = displayName;
        PrebidApiKey = prebidApiKey;
        AndroidPrebidApiKey = string.IsNullOrEmpty(androidPrebidApiKey) ? prebidApiKey : androidPrebidApiKey;
        SourceApp = sourceApp;
        OrgId = orgId;
        AppId = appId;
        IosPrebidHost = iosPrebidHost;
        AndroidPrebidHost = androidPrebidHost;
        IsInTestMode = isInTestMode;
        AppPackageName = appPackageName;
        AppVersionName = appVersionName;
        IosParameters = iosParameters;
        AndroidParameters = androidParameters;
        IosInterstitialPlacement = iosInterstitialPlacement;
        AndroidInterstitialPlacement = androidInterstitialPlacement;
        IosTestAdNetwork = iosTestAdNetwork;
        AndroidTestAdNetwork = androidTestAdNetwork;
    }

    public string InterstitialPlacement
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidInterstitialPlacement;
#else
            return IosInterstitialPlacement;
#endif
        }
    }

    public string TestAdNetwork
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidTestAdNetwork;
#else
            return IosTestAdNetwork;
#endif
        }
    }

    public MSPInitializationParameters ToMspInitializationParameters()
    {
        var parameters = new Dictionary<string, object>();
#if UNITY_ANDROID && !UNITY_EDITOR
        foreach (var entry in AndroidParameters)
            parameters[entry.Key] = entry.Value;
#else
        foreach (var entry in IosParameters)
            parameters[entry.Key] = entry.Value;
#endif

        var init = new MSPInitializationParameters
        {
            PrebidApiKey =
#if UNITY_ANDROID && !UNITY_EDITOR
                AndroidPrebidApiKey,
#else
                PrebidApiKey,
#endif
            SourceApp = SourceApp,
            OrgId = OrgId,
            AppId = AppId,
            PrebidHost =
#if UNITY_ANDROID && !UNITY_EDITOR
                AndroidPrebidHost,
#else
                IosPrebidHost,
#endif
            IsInTestMode = IsInTestMode,
            Parameters = parameters,
        };

#if UNITY_ANDROID && !UNITY_EDITOR
        init.AppPackageName = AppPackageName;
        init.AppVersionName = AppVersionName;
#endif

        return init;
    }
}

public static class GridLightAppProfiles
{
    private const string SelectedProfilePrefsKey = "gridlight_selected_app_profile";
    private const string SelectedAdNetworkPrefsKey = "gridlight_selected_ad_network";
    private const string SelectedPlacementPrefsKey = "gridlight_selected_placement";
    private const string AndroidNativePrefsName = "gridlight_app_profile_prefs";
    private const string AndroidNativeProfileKey = "selected_profile";

    private const string PrebidHostInternal = "https://prebid-server.newsbreak.com";
    private const string PrebidHostExternal = "https://msp.newsbreak.com";
    private const string AndroidPrebidHostInternal = "https://prebid-server.newsbreak.com/openrtb2/auction";
    private const string AndroidPrebidHostExternal = "https://msp.newsbreak.com/openrtb2/auction";

    private static readonly List<GridLightAppProfile> s_profiles = new List<GridLightAppProfile>();

    public static void Register(GridLightAppProfile profile)
    {
        if (profile == null) return;
        s_profiles.Add(profile);
    }

    static GridLightAppProfiles()
    {
        Register(new GridLightAppProfile(
            id: "demo-app",
            displayName: "Demo App",
            prebidApiKey: "af7ce3f9-462d-4df1-815f-09314bb87ca3",
            androidPrebidApiKey: null,
            sourceApp: "0000000000",
            orgId: 1061,
            appId: 1,
            iosPrebidHost: PrebidHostExternal,
            androidPrebidHost: AndroidPrebidHostExternal,
            isInTestMode: true,
            appPackageName: string.Empty,
            appVersionName: string.Empty,
            iosParameters: new Dictionary<string, object>
            {
                ["unityAppKey"] = "8545d445",
                ["inmobiAccountId"] = "4028cb8b2c3a0b45012c406824e800ba",
                ["mintegralAppId"] = "150180",
                ["mintegralApiKey"] = "7c22942b749fe6a6e361b675e96b3ee9",
                ["pubmaticPublisherId"] = "156276",
                ["pubmaticProfileIds"] = new[] { 1165 },
                ["pubmaticStoreUrl"] = "https://itunes.apple.com/us/app/pubmatic-sdk-app/id1175273098?mt=8",
                ["molocoAppKey"] = "NEWSBREAK:dX2DtwJM9o9okqwZ",
                ["liftoffAppId"] = "6937f2485cdd890926d69668",
                ["applovinSdkKey"] =
                    "6KrA5SQHFTBpGDUU4FeLIZGxGFmd1rORGfr5xlrJIMeXO8pdvuKPQO4WAfQpEZ4cXAOXoeSJJRoX0zcD4qBzak",
            },
            androidParameters: new Dictionary<string, object>
            {
                ["ppid"] = "shun-test-ppid",
                ["email"] = "shun.j@shun.com",
                ["unity_app_key"] = "207789bad",
                ["inmobi_account_id"] = "3ef8dd9e9d5b4080ad1682510980b643",
                ["mintegral_app_id"] = "144002",
                ["mintegral_app_key"] = "7c22942b749fe6a6e361b675e96b3ee9",
                ["pubmatic_publisher_id"] = "156276",
                ["moloco_app_key"] = "NEWSBREAK:tz5zGje2JXIAhpbZ",
                ["amazon_app_key"] = "369701c6-f17a-4573-b695-52aae43d960c",
                ["liftoff_app_id"] = "69437de9f9db799a8390058c",
                ["google_app_id"] = "ca-app-pub-3940256099942544~3347511713",
                ["applovin_sdk_key"] =
                    "6KrA5SQHFTBpGDUU4FeLIZGxGFmd1rORGfr5xlrJIMeXO8pdvuKPQO4WAfQpEZ4cXAOXoeSJJRoX0zcD4qBzak",
                ["prebidBidRequestTimeoutMillis"] = 50000,
            },
            iosInterstitialPlacement: "demo-ios-launch-fullscreen",
            androidInterstitialPlacement: "demo-android-interstitial-google-c2s-test",
            iosTestAdNetwork: "msp_fb",
            androidTestAdNetwork: "msp_google"));
    }

    private const string AllAdNetworkOption = "all";

    private static readonly string[] AdNetworkOptions =
    {
        AllAdNetworkOption,
        "msp_nova",
        "msp_google",
        "msp_fb",
        "vungle",
        "msp_moloco_native",
    };

    public static IReadOnlyList<GridLightAppProfile> All => s_profiles;
    public static IReadOnlyList<string> AdNetworks => AdNetworkOptions;

    public static GridLightAppProfile Current
    {
        get
        {
            var selectedId = LoadSelectedProfileId();
            foreach (var profile in s_profiles)
            {
                if (string.Equals(profile.Id, selectedId, StringComparison.Ordinal))
                    return profile;
            }

            return s_profiles.Count > 0 ? s_profiles[0] : null;
        }
    }

    public static void Save(GridLightAppProfile profile)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));

        PlayerPrefs.SetString(SelectedProfilePrefsKey, profile.Id);
        // Drop placement override so the new profile's default placement is used.
        PlayerPrefs.DeleteKey(SelectedPlacementPrefsKey);
        PlayerPrefs.Save();
        SaveAndroidNativeProfileId(profile.Id);
    }

    public static string CurrentAdNetwork
    {
        get
        {
            var saved = PlayerPrefs.GetString(SelectedAdNetworkPrefsKey, string.Empty);
            if (IsSupportedAdNetwork(saved)) return saved;

            if (IsSupportedAdNetwork(Current.TestAdNetwork)) return Current.TestAdNetwork;
            return AllAdNetworkOption;
        }
    }

    /// <summary>
    /// Value written to TestParams["ad_network"]. Empty when "all" is selected (no testParams).
    /// </summary>
    public static string CurrentTestAdNetwork
    {
        get
        {
            var selected = CurrentAdNetwork;
            return string.Equals(selected, AllAdNetworkOption, StringComparison.Ordinal)
                ? string.Empty
                : selected;
        }
    }

    public static string CurrentPlacement
    {
        get
        {
            var saved = PlayerPrefs.GetString(SelectedPlacementPrefsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(saved)) return saved.Trim();
            return Current.InterstitialPlacement;
        }
    }

    public static void SaveAdNetwork(string adNetwork)
    {
        if (!IsSupportedAdNetwork(adNetwork))
            throw new ArgumentException($"Unsupported ad network: {adNetwork}", nameof(adNetwork));

        PlayerPrefs.SetString(SelectedAdNetworkPrefsKey, adNetwork);
        PlayerPrefs.Save();
    }

    public static void SavePlacement(string placement)
    {
        if (string.IsNullOrWhiteSpace(placement))
            throw new ArgumentException("Placement must not be empty.", nameof(placement));

        PlayerPrefs.SetString(SelectedPlacementPrefsKey, placement.Trim());
        PlayerPrefs.Save();
    }

    public static string CurrentPrebidApiKey
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Current.AndroidPrebidApiKey;
#else
            return Current.PrebidApiKey;
#endif
        }
    }

    private static bool IsSupportedAdNetwork(string adNetwork)
    {
        if (string.IsNullOrEmpty(adNetwork)) return false;
        foreach (var option in AdNetworkOptions)
        {
            if (string.Equals(option, adNetwork, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string LoadSelectedProfileId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var nativeId = LoadAndroidNativeProfileId();
        if (!string.IsNullOrEmpty(nativeId))
            return nativeId;
#endif
        return PlayerPrefs.GetString(SelectedProfilePrefsKey, s_profiles.Count > 0 ? s_profiles[0].Id : null);
    }

    private static void SaveAndroidNativeProfileId(string profileId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null) return;

            using var prefs = activity.Call<AndroidJavaObject>(
                "getSharedPreferences",
                AndroidNativePrefsName,
                0);
            using var editor = prefs.Call<AndroidJavaObject>("edit");
            editor.Call<AndroidJavaObject>("putString", AndroidNativeProfileKey, profileId);
            editor.Call<bool>("commit");
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[GridLight Settings] Failed to commit Android profile prefs: {exception.Message}");
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static string LoadAndroidNativeProfileId()
    {
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null) return null;

            using var prefs = activity.Call<AndroidJavaObject>(
                "getSharedPreferences",
                AndroidNativePrefsName,
                0);
            return prefs.Call<string>("getString", AndroidNativeProfileKey, null);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[GridLight Settings] Failed to read Android profile prefs: {exception.Message}");
            return null;
        }
    }
#endif
}

public static class GridLightAppRestart
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void gridlight_exit_application();
#endif

    public static void CloseAfterProfileChange()
    {
#if UNITY_EDITOR
        Debug.Log("[GridLight Settings] Profile saved. App close is skipped in the Unity Editor.");
#elif UNITY_ANDROID
        // Match MSP Android demo: relaunch with CLEAR_TASK then exit the process.
        // finishAffinity + killProcess alone can leave the Unity process alive, so
        // DontDestroyOnLoad MSP state keeps the previous profile's API key.
        try
        {
            PlayerPrefs.Save();

            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null)
            {
                Application.Quit();
                return;
            }

            var packageName = activity.Call<string>("getPackageName");
            using var packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
            using var launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
            if (launchIntent != null)
            {
                const int flagActivityNewTask = 0x10000000;
                const int flagActivityClearTask = 0x00008000;
                launchIntent.Call<AndroidJavaObject>("addFlags", flagActivityNewTask | flagActivityClearTask);
                activity.Call("startActivity", launchIntent);
            }

            using var runtimeClass = new AndroidJavaClass("java.lang.Runtime");
            using var runtime = runtimeClass.CallStatic<AndroidJavaObject>("getRuntime");
            runtime.Call("exit", 0);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[GridLight Settings] Android process restart failed: {exception.Message}");
            Application.Quit();
        }
#elif UNITY_IOS
        gridlight_exit_application();
#else
        Application.Quit();
#endif
    }
}
