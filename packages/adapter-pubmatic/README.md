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

- Android (Maven Central): `ai.themsp:pubmatic-adapter:4.5.0`
- iOS (CocoaPods): `PubmaticAdapter` `4.5.0`
- iOS bootstrap registration for `PubmaticManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_pubmatic"`).
