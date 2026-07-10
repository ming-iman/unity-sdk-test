# MSP Unity Adapter - MobileFuse

Optional MobileFuse adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.mobilefuse": "file:../../packages/adapter-mobilefuse"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:mobilefuse-adapter:4.0.0`
- iOS (CocoaPods): `MobilefuseAdapter` `4.0.9`
- iOS bootstrap registration for `MobilefuseManager`

## Usage

Request ads with `ad_network = msp_mobilefuse` (or set `MSPAdRequest.AdNetwork`).
