# MSP Unity SDK — Demo Project

This is a sample Unity project that shows how to integrate the **MSP Unity SDK** into your own Unity app. It covers initialization, interstitial ad loading, and showing ads on both **Android** and **iOS**.

## Prerequisites

| Requirement | Notes |
|---|---|
| **Unity 2021.3+** | Tested on Unity 6000.x |
| **Android:** [EDM4U](https://github.com/googlesamples/unity-jar-resolver) | Resolved automatically as a UPM dependency |
| **iOS:** [CocoaPods](https://cocoapods.org) (`pod` on `PATH`) | Required to install native iOS pods |
| **Xcode 15+ / iOS 15+** | Deployment target for iOS builds |
| Network access to **Maven Central**, **Google Maven**, and **CocoaPods CDN** | Native dependencies are hosted on public registries |

## Quick Start

1. **Open** the `demo/` folder in Unity (Unity Hub → Add → select the `demo/` directory).

2. **Wait** for Unity to resolve packages. It pulls:
   - `ai.themsp.unity.core` (required — core SDK)
   - `ai.themsp.unity.adapter.nova`、`google`、`facebook`、`liftoff`、`moloco` (optional — ad network adapters)

3. **Open** `Assets/Scenes/SampleScene.unity`.

4. **Press Play** to run the mock ad flow in the Unity Editor (no device needed).

5. To test on a real device, **switch platform** to Android or iOS and build.

## How the Demo Uses the SDK

The demo comes with a **pre-configured test profile** so ads work out of the box. Once you're ready to use your own account, replace the test values with your own (see [Using Your Own Credentials](#using-your-own-credentials) below).

### Initialization

```csharp
MSP.Initialize(new MSPInitializationParameters
{
    PrebidApiKey = "YOUR_API_KEY",
    SourceApp = "YOUR_IOS_APP_STORE_ID",
    OrgId = YOUR_ORG_ID,
    AppId = YOUR_APP_ID,
    PrebidHost = "https://msp.newsbreak.com",
    IsInTestMode = true,
}, (success, message) => { /* handle result */ });
```

### Loading & Showing an Interstitial

```csharp
// Create a new loader for each ad load
var loader = new MSPAdLoader();
var request = new MSPAdRequest(placementId);

// Optional: force a specific ad network during testing
request.TestParams["test_ad"] = true;
request.TestParams["ad_network"] = "msp_nova";

loader.LoadAd(placementId, listener, request);

// In OnAdLoaded callback:
var ad = loader.GetAd(placementId) as MSPInterstitialAd;
ad?.Show();

// Dispose the loader when done
loader.Dispose();
```

### Key Demo Files

| File | Purpose |
|---|---|
| `Assets/Scripts/GridLightMspAds.cs` | Ad lifecycle: init → load → show → dismiss → reload |
| `Assets/Scripts/GridLightAppProfiles.cs` | App profile definition & `MSPInitializationParameters` mapping |

The remaining scripts in `Assets/Scripts/` are demo game logic — they are not MSP-specific and you do not need them in your own project.

## Using Your Own Credentials

Edit `Assets/Scripts/GridLightAppProfiles.cs` and replace the test values in the `"demo-app"` profile (or add a new profile):

- `prebidApiKey` — your Prebid API key
- `sourceApp` — your iOS App Store ID
- `orgId` / `appId` — your MSP organization & app IDs
- `iosInterstitialPlacement` / `androidInterstitialPlacement` — your placement IDs
- `iosParameters` / `androidParameters` — network-specific keys (Moloco, InMobi, PubMatic, etc.)

See the [MSP dashboard](https://msp.newsbreak.com) for your account details.

## Platform Setup

### Android

1. Open **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
2. If you use the **Google adapter**, add your AdMob App ID to the Android manifest:
   ```xml
   <meta-data android:name="com.google.android.gms.ads.APPLICATION_ID"
              android:value="ca-app-pub-xxxxxxxxxxxxxxxx~yyyyyyyyyy"/>
   ```
   Google provides [sample App IDs](https://developers.google.com/admob/android/test-ads) for testing.
3. Build and run.

### iOS

1. Switch platform to **iOS** in Build Settings.
2. **Build** — Unity exports an Xcode project and runs `pod install` automatically.
3. Open `Unity-iPhone.xcworkspace` and run on a device.

If CocoaPods fails during the build, set `MSP_UNITY_SKIP_POD_INSTALL=1` in your environment, then run `pod install` manually from the exported Xcode project directory.

## Supported Ad Networks

Install the corresponding adapter package for each network you want to use:

| Adapter Package | Test Ad Network Key |
|---|---|
| `ai.themsp.unity.adapter.nova` | `msp_nova` |
| `ai.themsp.unity.adapter.google` | `msp_google` |
| `ai.themsp.unity.adapter.facebook` | `msp_fb` |
| `ai.themsp.unity.adapter.moloco` | `msp_moloco_native` |
| `ai.themsp.unity.adapter.liftoff` | (Liftoff / Vungle) |
| `ai.themsp.unity.adapter.unity` | (Unity Ads) |
| `ai.themsp.unity.adapter.inmobi` | (InMobi) |
| `ai.themsp.unity.adapter.mintegral` | (Mintegral) |
| `ai.themsp.unity.adapter.pubmatic` | (PubMatic) |
| `ai.themsp.unity.adapter.amazon` | (Amazon) |
| `ai.themsp.unity.adapter.applovin` | (AppLovin) |

## Integrating Into Your Own Project

1. Add `ai.themsp.unity.core` to your project's `Packages/manifest.json`:
   ```json
   {
     "dependencies": {
       "ai.themsp.unity.core": "https://github.com/ming-iman/unity-sdk-test.git?path=/upm#v4.5.0-rc.2"
     }
   }
   ```

2. Add adapter packages for the networks you need:
   ```json
   "ai.themsp.unity.adapter.nova": "https://github.com/ming-iman/unity-sdk-test.git?path=/packages/adapter-nova#v4.5.0-rc.2"
   ```
   > **Important:** Use the **same version tag** for all MSP packages.

3. Copy the initialization and ad-loading patterns from `GridLightMspAds.cs` into your own code.

4. **Android:** Run **Assets → EDM → Android Resolver → Force Resolve** after adding adapters.

5. **iOS:** Build the Xcode project — the postprocess step handles CocoaPods automatically.

## Troubleshooting

### Packages fail to resolve

- Make sure your network can reach GitHub, Maven Central, and CocoaPods CDN.
- Git tag URLs are case-sensitive — verify the tag matches exactly.
- Try deleting `Library/PackageCache` and `Packages/packages-lock.json`, then reopen Unity.

### Android build fails

- Run **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
- Make sure you are using a compatible Gradle template (Unity 2021.3+ includes one by default).

### iOS build fails

- Verify CocoaPods is installed: `which pod`.
- If the build times out fetching pods, set `MSP_UNITY_SKIP_POD_INSTALL=1` and run `pod install` manually in the exported Xcode project folder.

### Ads don't load

- Set `IsInTestMode = true` during development.
- Check the Unity console for `[GridLight MSP]` log messages.
- Install at least one adapter package — the core package alone has no ad networks.
- On Android, verify EDM resolved native dependencies (check `Assets/Plugins/Android`).
