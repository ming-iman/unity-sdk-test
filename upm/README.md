# MSP Unity SDK

Unity Package Manager plugin (`ai.themsp.unity`) for MSP ads.

Monorepo layout: see [../README.md](../README.md).

## Scope (MVP)

- Interstitial only
- Keep MSP public API semantics unchanged:
  - `Initialize`
  - `LoadAd`
  - `GetAd`
  - `Show`

## Status

- Git repo initialized locally
- UPM-style structure created
- Android bridge integrated and validated on demo app
- iOS bridge wired with CocoaPods postprocess for demo export
- Editor mock flow available for local demo
