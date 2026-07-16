# MSP Unity Adapter - Nova

Optional Nova adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.nova": "file:../../packages/adapter-nova"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:nova-adapter:4.5.0`
- iOS (CocoaPods trunk): `MSPNovaAdapter` `4.5.0` (includes `NovaCore` xcframework)
- iOS bootstrap registration for `NovaManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_nova"`).
