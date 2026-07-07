# MSP Unity SDK Publishing Layout

This document describes the publishable layout for MSP Unity SDK, starting with the Nova adapter.

## Package matrix

| Package | Path | Required |
|---|---|---|
| `ai.themsp.unity.core` | `upm/` | Yes |
| `ai.themsp.unity.adapter.nova` | `packages/adapter-nova/` | Optional |

## Native dependency sources (external users)

| Platform | Source | Version pin |
|---|---|---|
| Android | [Maven Central](https://central.sonatype.com/search?namespace=ai.themsp) (`ai.themsp:*`) | `4.0.0` |
| iOS | [CocoaPods trunk](https://cocoapods.org) (`MSPCore`, `MSPNovaAdapter`, …) | `4.0.9` |

Version pins live in `upm/Runtime/Adapter/MSPUnityNativeVersions.cs`.

No internal Artifactory account or sibling `msp-ios-sdk` checkout is required for external integration.

### Local monorepo dev override (optional)

| Env var | Effect |
|---|---|
| `MSP_UNITY_USE_LOCAL_IOS_SDK=1` | Podfile uses local `:path` pods (optional `MSP_IOS_SDK_PATH` override) |
| `MSP_IOS_SDK_PATH=/path/to/msp-ios-sdk` | Used for `bundle exec pod install` Gemfile only; does **not** switch Podfile to local pods unless `MSP_UNITY_USE_LOCAL_IOS_SDK=1` |

## User install (local monorepo)

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.nova": "file:../../packages/adapter-nova"
  }
}
```

## User install (git tag release)

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "https://github.com/<org>/msp-unity-sdk.git?path=/upm#v0.2.0",
    "ai.themsp.unity.adapter.nova": "https://github.com/<org>/msp-unity-sdk.git?path=/packages/adapter-nova#v0.2.0"
  }
}
```

## Responsibility split

### Core (`upm/`)

- Unity C# API (`MSP`, `MSPAdLoader`, interstitial flow)
- Android/iOS bridge (`MSPUnityBridge`, `MSPUnityEntry`)
- Android bridge AAR bundled at `upm/Plugins/Android/msp-unity-bridge-release.aar`
- Adapter registry (`MSPUnityAdapterRegistry`)
- iOS postprocess Podfile builder (CocoaPods trunk by default)
- Android core dependencies (`msp-core`, `prebid-adapter`) via Maven Central

### Nova adapter (`packages/adapter-nova/`)

- Registers itself into `MSPUnityAdapterRegistry`
- Android: `ai.themsp:nova-adapter:4.0.0` (Maven Central)
- iOS: `MSPNovaAdapter` `4.0.9` (CocoaPods trunk; bundles `NovaCore`)
- iOS bootstrap: `msp_unity_register_adapter_nova` registers `NovaManager`

## Runtime flow

1. User installs `core` + `adapter-nova`
2. Nova package registers adapter metadata at editor/runtime load
3. `MSP.Initialize()` activates iOS adapter registration, then initializes MSP
4. iOS export generates Podfile from registry and runs `pod install`
5. Android EDM resolves core + nova artifacts from Maven Central

## Prerequisites for external users

### Android

- Unity External Dependency Manager (EDM4U)
- Network access to Maven Central and Google Maven
- No Artifactory credentials required

### iOS

- CocoaPods installed (`pod` in PATH)
- Network access to CocoaPods CDN and `github.com/ParticleMedia/msp-ios-sdk-public` release zips
- Xcode 15+ / iOS 15+ deployment target
- Trunk MSP pods ship **dynamic** xcframeworks; the Unity export postprocess embeds them into the app bundle automatically

## Next adapters

Add new packages under `packages/adapter-<name>/` using the same pattern:

- `Runtime/<Name>AdapterContributor.cs`
- `Editor/Dependencies.xml` (Maven Central coordinates only)
- `Plugins/iOS/MSPUnity<Name>Bootstrap.swift` (if native manager registration is needed)
- Pin iOS pod version in contributor via `MSPUnityNativeVersions` or adapter-specific constant

## Release checklist

1. Verify public native versions (`MSPUnityNativeVersions`) match Maven Central / CocoaPods trunk
2. Bump versions in `upm/package.json` and adapter `package.json`
3. Run `tools/release/build-packages.sh` (builds Android bridge AAR, validates, packs tgz files into `build/`)
4. Tag repo (`v0.0.1-rc.0`)
5. Validate in a clean Unity project:
   - core only (should build, but no nova ads)
   - core + nova (should load/show nova interstitial)
