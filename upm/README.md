# MSP Unity SDK Core

Unity Package Manager plugin (`ai.themsp.unity.core`) for MSP ads.

Monorepo layout: see [../README.md](../README.md).
Publishing layout: see [../docs/publishing-layout.md](../docs/publishing-layout.md).

## Scope (MVP)

- Interstitial only
- Keep MSP public API semantics unchanged:
  - `Initialize`
  - `LoadAd`
  - `GetAd`
  - `Show`

## Adapter model

- Core includes Unity API, bridge, and adapter registry.
- Optional adapters are separate UPM packages under `../packages/adapter-*`.
  Supported: Nova, Google, Facebook, Unity Ads, InMobi, MobileFuse, Mintegral, PubMatic, Moloco, Amazon, Liftoff, AppLovin.

## Status

- Interstitial MVP on Android / iOS
- Modular adapters under `packages/adapter-*`
- Public native deps: Maven Central (Android `4.5.0`) + CocoaPods trunk (iOS `4.5.0`)
- Editor mock flow for Play Mode without native SDKs
