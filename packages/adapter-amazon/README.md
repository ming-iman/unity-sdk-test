# MSP Unity Adapter - Amazon

Optional Amazon adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.amazon": "file:../../packages/adapter-amazon"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:amazon-adapter:4.0.0`
- iOS (CocoaPods): `MSPAmazonAdapter` `4.0.9`
- iOS bootstrap registration for `AmazonManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_amazon"`).
