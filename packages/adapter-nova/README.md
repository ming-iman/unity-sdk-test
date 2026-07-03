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

- Android Maven dependency: `ai.themsp:nova-adapter`
- iOS CocoaPods: `MSPNovaAdapter`, `NovaCore`, `MSPKingfisher`, `MSPSnapKit`
- iOS bootstrap registration for `NovaManager`

## Usage

Request ads with `ad_network = msp_nova` (or set `MSPAdRequest.AdNetwork`).
