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
- Optional adapters (Nova first) are separate UPM packages under `../packages/`.

## Status

- Git repo initialized locally
- UPM-style structure created
- Android bridge integrated and validated on demo app
- iOS bridge wired with CocoaPods postprocess for demo export
- Nova adapter package for modular publishing
- Public native deps: Maven Central (Android `4.0.0`) + CocoaPods trunk (iOS `4.0.9`)
- Editor mock flow available for local demo
