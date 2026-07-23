# MSP Unity SDK Publishing Layout

This document describes the publishable layout for MSP Unity SDK and optional adapters.

## Package matrix

| Package | Path | Required |
|---|---|---|
| `ai.themsp.unity.core` | `upm/` | Yes |
| `ai.themsp.unity.adapter.nova` | `packages/adapter-nova/` | Optional |
| `ai.themsp.unity.adapter.google` | `packages/adapter-google/` | Optional |
| `ai.themsp.unity.adapter.facebook` | `packages/adapter-facebook/` | Optional |
| `ai.themsp.unity.adapter.unity` | `packages/adapter-unity/` | Optional (Unity Ads) |
| `ai.themsp.unity.adapter.inmobi` | `packages/adapter-inmobi/` | Optional |
| `ai.themsp.unity.adapter.mobilefuse` | `packages/adapter-mobilefuse/` | Optional |
| `ai.themsp.unity.adapter.mintegral` | `packages/adapter-mintegral/` | Optional |
| `ai.themsp.unity.adapter.pubmatic` | `packages/adapter-pubmatic/` | Optional |
| `ai.themsp.unity.adapter.moloco` | `packages/adapter-moloco/` | Optional |
| `ai.themsp.unity.adapter.amazon` | `packages/adapter-amazon/` | Optional |
| `ai.themsp.unity.adapter.liftoff` | `packages/adapter-liftoff/` | Optional |
| `ai.themsp.unity.adapter.applovin` | `packages/adapter-applovin/` | Optional |

## Native dependency sources

| Platform | Source | Version pin |
|---|---|---|
| Android | [Maven Central](https://central.sonatype.com/search?namespace=ai.themsp) (`ai.themsp:*`) | `4.5.0` |
| iOS | [CocoaPods trunk](https://cocoapods.org) (`MSPCore`, adapter pods, …) | `4.5.0` |

Version pins live in `upm/Runtime/Adapter/MSPUnityNativeVersions.cs`.

Integrators do not need private Maven repositories or a local native SDK checkout.

### Adapter native coordinates

| Adapter id | Android Maven | iOS pod | Notes |
|---|---|---|---|
| `nova` | `ai.themsp:nova-adapter` | `MSPNovaAdapter` | |
| `google` | `ai.themsp:google-adapter` | `MSPGoogleAdapter` | Requires Google Ads App ID |
| `facebook` | `ai.themsp:facebook-adapter` | `MSPFacebookAdapter` | |
| `unity` | `ai.themsp:unity-adapter` | `UnityAdapter` | Unity Ads network |
| `inmobi` | `ai.themsp:inmobi-adapter` | `InmobiAdapter` | |
| `mobilefuse` | `ai.themsp:mobilefuse-adapter` | `MobilefuseAdapter` | |
| `mintegral` | `ai.themsp:mintegral-adapter` | `MintegralAdapter` | |
| `pubmatic` | `ai.themsp:pubmatic-adapter` | `PubmaticAdapter` | |
| `moloco` | `ai.themsp:moloco-adapter` | `MSPMolocoAdapter` | |
| `amazon` | `ai.themsp:amazon-adapter` | `MSPAmazonAdapter` | |
| `liftoff` | `ai.themsp:liftoff-adapter` | `MSPLiftoffAdapter` | |
| `applovin` | `ai.themsp:applovin-adapter` | `MSPApplovinMaxAdapter` | |

Some iOS adapter pods may not yet be published on CocoaPods trunk for every version; for local native SDK development set `MSP_UNITY_USE_LOCAL_IOS_SDK=1` and `MSP_IOS_SDK_PATH`.

### Local iOS SDK override (optional, developers only)

| Env var | Effect |
|---|---|
| `MSP_UNITY_USE_LOCAL_IOS_SDK=1` | Podfile uses local `:path` pods |
| `MSP_IOS_SDK_PATH=/path/to/ios-sdk` | Required when local mode is enabled; also used for `bundle exec pod install` Gemfile lookup |

## User install (local monorepo)

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.nova": "file:../../packages/adapter-nova",
    "ai.themsp.unity.adapter.google": "file:../../packages/adapter-google"
  }
}
```

Install only the adapters you need. Core alone builds; ads for a network require that network's adapter package.

## User install (git tag)

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "https://github.com/ming-iman/unity-sdk-test.git?path=/upm#v4.5.0-rc.4",
    "ai.themsp.unity.adapter.nova": "https://github.com/ming-iman/unity-sdk-test.git?path=/packages/adapter-nova#v4.5.0-rc.4"
  }
}
```

Use the same tag for every package. Push matching `v*` tags to the remote that hosts these URLs (`git push public vX.Y.Z` when using the `public` remote).

## User install (tarball)

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../build/ai.themsp.unity.core-4.5.0-rc.4.tgz",
    "ai.themsp.unity.adapter.nova": "file:../build/ai.themsp.unity.adapter.nova-4.5.0-rc.4.tgz"
  }
}
```

Release tarballs are produced under `build/` by `tools/release/build-packages.sh`.

## Responsibility split

### Core (`upm/`)

- Unity C# API (`MSP`, `MSPAdLoader`, interstitial flow)
- Android/iOS bridge (`MSPUnityBridge`, `MSPUnityEntry`)
- Android bridge AAR bundled at `upm/Plugins/Android/msp-unity-bridge-release.aar`
- Adapter registry (`MSPUnityAdapterRegistry`)
- iOS postprocess Podfile builder (CocoaPods trunk by default)
- Android core dependencies (`msp-core`, `prebid-adapter`) via Maven Central

### Optional adapters (`packages/adapter-*`)

Each adapter package:

- Registers itself into `MSPUnityAdapterRegistry`
- Declares Android Maven coordinates in `Editor/Dependencies.xml`
- Declares iOS pod + bootstrap (`Plugins/iOS/MSPUnity*Bootstrap.swift`)
- Is independently installable

## Runtime flow

1. User installs `core` + one or more adapter packages
2. Adapter packages register metadata at editor/runtime load
3. `MSP.Initialize()` activates iOS adapter registration, then initializes MSP
4. iOS export generates Podfile from registry and runs `pod install`
5. Android EDM resolves core + selected adapter artifacts from Maven Central

On iOS, when the Google / Facebook / Moloco / Liftoff Unity adapter packages are installed, their native bootstrap also assigns the corresponding bid-token helpers on `MSP.shared.bidLoaderProvider` (same wiring as the native iOS demo). Android adapter AARs expose token providers that `BidLoaderProviderImp` loads by reflection.

## Prerequisites for external users

### Android

- Unity External Dependency Manager (EDM4U)
- Network access to Maven Central and Google Maven
- Google adapter: set `com.google.android.gms.ads.APPLICATION_ID` in AndroidManifest

### iOS

- CocoaPods installed (`pod` in PATH)
- Network access to CocoaPods CDN (MSP pods resolve public release artifacts)
- Xcode 15+ / iOS 15+ deployment target
- Trunk MSP pods ship **dynamic** xcframeworks; the Unity export postprocess embeds them into the app bundle automatically
- Google adapter: GAD App ID is injected by postprocess when Google is registered (`MSP_UNITY_GAD_APP_ID` or Google sample default)

## Adding future adapters

Add new packages under `packages/adapter-<name>/` using the same pattern:

- `Runtime/<Name>AdapterContributor.cs`
- `Editor/Dependencies.xml` (Maven Central package specs only — no repository URLs)
- `Plugins/iOS/MSPUnity<Name>Bootstrap.swift` (native manager registration)
- Register assembly in `MSPUnityOptionalAdapterLoader`
- Add package name to `MSPUnityIosAdapterBootstrapEnsurer`
- Add adapter id to `MSPUnityEntry.linkedOptionalAdapterIds`
- Pin iOS pod version via `MSPUnityNativeVersions`

## Release checklist

1. Ensure working tree is clean
2. Run `./tools/release/publish.sh <version>` (writes `VERSION`, syncs `package.json`, validates, commits, tags `v<version>`, pushes `origin` + `public`)
3. Optional: `./tools/release/publish.sh <version> --pack` to also rebuild the bridge AAR and emit `build/*.tgz`
4. Verify public native versions (`MSPUnityNativeVersions`) match Maven Central / CocoaPods trunk when bumping those pins
5. Validate in a clean Unity project:
   - core only (should build, but no network-specific ads)
   - core + selected adapters (should load/show for those networks)

Do not hand-edit version fields in individual `package.json` files; `sync-package-versions.sh` overwrites them from `tools/release/VERSION`.
