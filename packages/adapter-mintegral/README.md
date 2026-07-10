# MSP Unity Adapter - Mintegral

Optional Mintegral adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.mintegral": "file:../../packages/adapter-mintegral"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:mintegral-adapter:4.0.0`
- iOS (CocoaPods): `MintegralAdapter` `4.0.9`
- iOS bootstrap registration for `MintegralManager`

## Usage

Request ads with `ad_network = msp_mintegral` (or set `MSPAdRequest.AdNetwork`).
