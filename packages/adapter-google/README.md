# MSP Unity Adapter - Google

Optional Google adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.google": "file:../../packages/adapter-google"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:google-adapter:4.5.0`
- iOS (CocoaPods): `MSPGoogleAdapter` `4.5.0`
- iOS bootstrap registration for `GoogleManager`

## Usage

Put `ad_network` in `MSPAdRequest.TestParams` (e.g. `request.TestParams["ad_network"] = "msp_google"`).

On iOS, installing this package also wires `GoogleQueryInfoFetcherHelper` into `MSP.shared.bidLoaderProvider` when the adapter activates (same as the native iOS demo). Android resolves `GoogleBidTokenProvider` automatically when the Maven adapter is on the classpath.
