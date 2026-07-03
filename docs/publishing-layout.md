# MSP Unity SDK Publishing Layout

This document describes the first publishable layout for MSP Unity SDK, starting with the Nova adapter.

## Package matrix

| Package | Path | Required |
|---|---|---|
| `ai.themsp.unity.core` | `upm/` | Yes |
| `ai.themsp.unity.adapter.nova` | `packages/adapter-nova/` | Optional |

## User install (local monorepo)

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "file:../../upm",
    "ai.themsp.unity.adapter.nova": "file:../../packages/adapter-nova"
  }
}
```

## User install (git tag release)

```json
{
  "dependencies": {
    "ai.themsp.unity.core": "https://github.com/<org>/msp-unity-sdk.git?path=/upm#v0.2.0",
    "ai.themsp.unity.adapter.nova": "https://github.com/<org>/msp-unity-sdk.git?path=/packages/adapter-nova#v0.2.0"
  }
}
```

## Responsibility split

### Core (`upm/`)

- Unity C# API (`MSP`, `MSPAdLoader`, interstitial flow)
- Android/iOS bridge (`MSPUnityBridge`, `MSPUnityEntry`)
- Adapter registry (`MSPUnityAdapterRegistry`)
- iOS postprocess Podfile builder (core pods + enabled adapters)
- Android core dependencies (`msp-core`, `prebid-adapter`) via `upm/Editor/Dependencies.xml`

### Nova adapter (`packages/adapter-nova/`)

- Registers itself into `MSPUnityAdapterRegistry`
- Android dependency: `ai.themsp:nova-adapter`
- iOS pods: `MSPNovaAdapter`, `NovaCore`, `MSPKingfisher`, `MSPSnapKit`
- iOS bootstrap: `MSPUnityNovaBootstrap.activate()` registers `NovaManager`

## Runtime flow

1. User installs `core` + `adapter-nova`
2. Nova package registers adapter metadata at editor/runtime load
3. `MSP.Initialize()` activates iOS bootstrap classes, then initializes MSP
4. iOS export generates Podfile from registry (no Google pods when Nova-only)
5. Android EDM resolves core + nova Maven artifacts

## Next adapters

Add new packages under `packages/adapter-<name>/` using the same pattern:

- `Runtime/<Name>AdapterContributor.cs`
- `Editor/Dependencies.xml`
- `Plugins/iOS/MSPUnity<Name>Bootstrap.swift` (if native manager registration is needed)

## Release checklist

1. Bump versions in `upm/package.json` and adapter `package.json`
2. Run `tools/release/validate-packages.sh`
3. Tag repo (`v0.2.0`)
4. Validate in a clean Unity project:
   - core only (should build, but no nova ads)
   - core + nova (should load/show nova interstitial)
