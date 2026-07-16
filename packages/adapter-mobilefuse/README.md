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

- Android (Maven Central): `ai.themsp:mobilefuse-adapter:4.5.0`
- iOS (CocoaPods): `MobilefuseAdapter` `4.5.0`
- iOS bootstrap registration for `MobilefuseManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_mobilefuse"`).
