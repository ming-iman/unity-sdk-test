# MSP Unity SDK

Unity Package Manager (UPM) SDK for MSP interstitial ads on Android and iOS.

- **Unity packages:** `ai.themsp.unity.core` + optional `ai.themsp.unity.adapter.*`
- **Native Android:** Maven Central (`ai.themsp:*`)
- **Native iOS:** CocoaPods trunk (`MSPCore` and adapter pods)
- **Current package version:** see `tools/release/VERSION` (now `0.0.1-rc.2`)

## Project structure

```
msp-unity-sdk/
├── upm/                         # ai.themsp.unity.core (required UPM package)
│   ├── Runtime/                 # C# API: MSP, MSPAdLoader, interstitial
│   ├── Editor/                  # iOS postprocess, EDM Dependencies.xml
│   └── Plugins/
│       ├── Android/             # shipped msp-unity-bridge-release.aar
│       └── iOS/                 # Swift / ObjC bridge
├── packages/
│   └── adapter-*/               # optional network adapters (Nova, Google, …)
├── android-bridge/              # Gradle module that builds the bridge AAR
├── demo/                        # sample Unity project
├── docs/publishing-layout.md    # package matrix & native coordinates
├── tools/release/               # VERSION, validate, sync, pack
└── build/                       # generated .tgz (gitignored)
```

| Path | Role |
|---|---|
| `upm/` | Publishable core package |
| `packages/adapter-*` | Optional adapters; install only what you need |
| `android-bridge/` | Builds / updates the Android bridge AAR |
| `demo/` | End-to-end integration sample |
| `tools/release/VERSION` | Single source of Unity package version |

Adapter list and native Maven/pod names: [`docs/publishing-layout.md`](docs/publishing-layout.md).

## Requirements

- Unity **2021.3+** (demo tested on Unity 6000.x)
- **Android:** [External Dependency Manager for Unity (EDM4U)](https://github.com/googlesamples/unity-jar-resolver)
- **iOS:** CocoaPods (`pod` on `PATH`), Xcode 15+, iOS 15+ deployment target
- Network access to **Maven Central**, **Google Maven**, and **CocoaPods CDN**

## Integrate into a Unity project

### 1. Add packages

Install **core** plus the adapters you need via one of:

**A. Git tag (recommended for remote install)**

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "https://github.com/ming-iman/unity-sdk-test.git?path=/upm#v0.0.1-rc.2",
    "ai.themsp.unity.adapter.nova": "https://github.com/ming-iman/unity-sdk-test.git?path=/packages/adapter-nova#v0.0.1-rc.2",
    "com.google.external-dependency-manager": "https://github.com/googlesamples/unity-jar-resolver.git?path=upm"
  }
}
```

Use the **same tag** for every MSP package. Change the GitHub host/path when you publish from another remote.

**B. Local monorepo (day-to-day SDK development)**

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.nova": "file:../../packages/adapter-nova"
  }
}
```

**C. Tarball**

```bash
./tools/release/build-packages.sh
```

Then in `Packages/manifest.json`:

```json
"ai.themsp.unity.core": "file:../build/ai.themsp.unity.core-0.0.1-rc.2.tgz",
"ai.themsp.unity.adapter.nova": "file:../build/ai.themsp.unity.adapter.nova-0.0.1-rc.2.tgz"
```

### 2. Android

1. Ensure EDM4U is installed.
2. Enable a custom Gradle settings template if required by your Unity version.
3. **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
4. If you use the Google adapter, set `com.google.android.gms.ads.APPLICATION_ID` in the Android manifest (Google provides sample App IDs for testing).

The bridge AAR ships inside `ai.themsp.unity.core` at `Plugins/Android/msp-unity-bridge-release.aar`. Native MSP / mediation libs resolve from Maven Central.

### 3. iOS

1. Switch platform to iOS and **Build** (export Xcode project).
2. Postprocess generates a `Podfile` from installed adapters and runs `pod install` (CocoaPods trunk by default).
3. Open `Unity-iPhone.xcworkspace` and run on device.

Optional env vars:

| Variable | Effect |
|---|---|
| `MSP_UNITY_SKIP_POD_INSTALL=1` | Skip automatic `pod install` |
| `MSP_UNITY_USE_LOCAL_IOS_SDK=1` | Use local `:path` pods (requires `MSP_IOS_SDK_PATH`) |
| `MSP_IOS_SDK_PATH=/path/to/ios-sdk` | Local MSP iOS SDK checkout for pod / Gemfile override |
| `MSP_UNITY_GAD_APP_ID=…` | Override Google Ads App ID written into `Info.plist` |

### 4. Minimal C# usage

```csharp
MSP.Initialize(new MSPInitializationParameters
{
    PrebidApiKey = "YOUR_API_KEY",
    OrgId = YOUR_ORG_ID,
    AppId = YOUR_APP_ID,
    IsInTestMode = true
}, (success, message) => { /* … */ });

var loader = new MSPAdLoader();
var request = new MSPAdRequest(placementId);
request.TestParams["test_ad"] = true;
request.TestParams["ad_network"] = "msp_nova";
// request.CustomParams["your_key"] = "your_value";
loader.LoadAd(placementId, listener, request);
var ad = loader.GetAd(placementId) as MSPInterstitialAd;
ad?.Show();
```

API scope today: **interstitial only** (`Initialize` / `LoadAd` / `GetAd` / `Show`).

### 5. Demo project

Open `demo/` in Unity. The sample resolves MSP packages from the public git tag shown above. For local iteration, switch `Packages/manifest.json` back to `file:../../upm` and `file:../../packages/adapter-nova`.

## Release

Unity package version is owned by **one file**: `tools/release/VERSION`.

```bash
# 1) Set version
echo '0.0.2-rc.0' > tools/release/VERSION

# 2) Sync package.json, build bridge AAR into upm/, validate, pack .tgz → build/
./tools/release/build-packages.sh

# 3) Commit, then tag to match VERSION
git tag -a "v0.0.2-rc.0" -m "MSP Unity SDK 0.0.2-rc.0"
git push origin main
git push origin "v0.0.2-rc.0"

# Optional: mirror for public git-link installs
git push public main
git push public "v0.0.2-rc.0"
```

Notes:

- Do **not** hand-edit `version` in individual `package.json` files; `sync-package-versions.sh` overwrites them from `VERSION`.
- Native MSP versions (Android Maven / iOS pods) live in `upm/Runtime/Adapter/MSPUnityNativeVersions.cs` and are independent of the Unity package version.
- Remotes: `origin` (primary), optional `public` for a public mirror used by git `#tag` URLs.
- Full checklist and package matrix: [`docs/publishing-layout.md`](docs/publishing-layout.md).

## Build Android bridge only

```bash
cd android-bridge
make install                          # → demo/Assets/Plugins/Android/
UNITY_PLUGINS_DIR=../upm/Plugins/Android make install   # → core package
```

Requires `./gradlew` or `gradle` on `PATH` (override with `GRADLEW=…`).

## More docs

- [`docs/publishing-layout.md`](docs/publishing-layout.md) — adapters, native coordinates, install variants
- [`upm/README.md`](upm/README.md) — core package status / API notes
- [`android-bridge/README.md`](android-bridge/README.md) — AAR build details
