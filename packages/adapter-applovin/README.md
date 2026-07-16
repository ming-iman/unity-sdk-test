# MSP Unity Adapter - AppLovin

Optional AppLovin adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.applovin": "file:../../packages/adapter-applovin"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:applovin-adapter:4.5.0`
- iOS (CocoaPods): `MSPApplovinMaxAdapter` `4.5.0`
- iOS bootstrap registration for `ApplovinMaxManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_applovin"`).
