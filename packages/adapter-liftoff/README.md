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

- Android (Maven Central): `ai.themsp:liftoff-adapter:4.5.0`
- iOS (CocoaPods): `MSPLiftoffAdapter` `4.5.0`
- iOS bootstrap registration for `LiftoffManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_liftoff"`).

On iOS, installing this package also wires `LiftoffBidTokenProviderHelper` into `MSP.shared.bidLoaderProvider` when the adapter activates (same as the native iOS demo). Android resolves `LiftoffBidTokenProvider` automatically when the Maven adapter is on the classpath.
