# MSP Unity Adapter - PubMatic

Optional PubMatic adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.pubmatic": "file:../../packages/adapter-pubmatic"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:pubmatic-adapter:4.0.0`
- iOS (CocoaPods): `PubmaticAdapter` `4.0.9`
- iOS bootstrap registration for `PubmaticManager`

## Usage

Request ads with `ad_network = msp_pubmatic` (or set `MSPAdRequest.AdNetwork`).
