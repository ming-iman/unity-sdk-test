# MSP Unity Adapter - InMobi

Optional InMobi adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.inmobi": "file:../../packages/adapter-inmobi"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:inmobi-adapter:4.5.0`
- iOS (CocoaPods): `InmobiAdapter` `4.5.0`
- iOS bootstrap registration for `InmobiManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_inmobi"`).
