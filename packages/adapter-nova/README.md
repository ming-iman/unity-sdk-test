# MSP Unity Adapter - Nova

Optional Nova adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.nova": "file:../../packages/adapter-nova"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:nova-adapter:4.0.0`
- iOS (CocoaPods trunk): `MSPNovaAdapter` `4.0.9` (includes `NovaCore` xcframework)
- iOS bootstrap registration for `NovaManager`

## Usage

Request ads with `ad_network = msp_nova` (or set `MSPAdRequest.AdNetwork`).
