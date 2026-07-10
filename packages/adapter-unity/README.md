# MSP Unity Adapter - Unity Ads

Optional Unity Ads adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.unity": "file:../../packages/adapter-unity"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:unity-adapter:4.0.0`
- iOS (CocoaPods): `UnityAdapter` `4.0.9`
- iOS bootstrap registration for `UnityManager`

## Usage

Request ads with `ad_network = msp_unity` (or set `MSPAdRequest.AdNetwork`).
