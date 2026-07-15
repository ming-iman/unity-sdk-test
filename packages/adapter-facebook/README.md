# MSP Unity Adapter - Facebook

Optional Facebook adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.facebook": "file:../../packages/adapter-facebook"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:facebook-adapter:4.0.0`
- iOS (CocoaPods): `MSPFacebookAdapter` `4.0.9`
- iOS bootstrap registration for `FacebookManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_facebook"`).
