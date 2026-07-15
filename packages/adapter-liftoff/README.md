# MSP Unity Adapter - Liftoff

Optional Liftoff adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.liftoff": "file:../../packages/adapter-liftoff"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:liftoff-adapter:4.0.0`
- iOS (CocoaPods): `MSPLiftoffAdapter` `4.0.9`
- iOS bootstrap registration for `LiftoffManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_liftoff"`).
