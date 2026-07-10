# MSP Unity Adapter - Moloco

Optional Moloco adapter for MSP Unity SDK.

## Install

Add both packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.moloco": "file:../../packages/adapter-moloco"
  }
}
```

## What this package adds

- Android (Maven Central): `ai.themsp:moloco-adapter:4.0.0`
- iOS (CocoaPods): `MSPMolocoAdapter` `4.0.9`
- iOS bootstrap registration for `MolocoManager`

## Usage

Request ads with `ad_network = msp_moloco` (or set `MSPAdRequest.AdNetwork`).
