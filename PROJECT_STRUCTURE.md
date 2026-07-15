# MSP Unity SDK Directory Structure

Brief map of directories. For integration and release steps, see the root [`README.md`](README.md).

## Top level

| Directory | Purpose |
|---|---|
| `upm/` | Core UPM package (`ai.themsp.unity.core`) |
| `packages/` | Optional adapter UPM packages |
| `android-bridge/` | Gradle project that builds `msp-unity-bridge-release.aar` |
| `demo/` | Unity sample app |
| `docs/` | Publishing / layout details |
| `tools/release/` | Version file, validate, sync, pack scripts |
| `build/` | Generated `.tgz` artifacts (gitignored) |

## Details

- `upm/Runtime` — public C# API and adapter registry
- `upm/Editor` — iOS postprocess, dependency XML, editor helpers
- `upm/Plugins/Android` — shipped bridge AAR
- `upm/Plugins/iOS` — Swift / ObjC bridge
- `packages/adapter-*` — one folder per network (`Runtime`, `Editor`, `Plugins/iOS`)
- `android-bridge/bridge` — Android library module
- `demo/Assets` — sample scene and Android plugin config
- `demo/Packages` — Unity manifest / lock file
