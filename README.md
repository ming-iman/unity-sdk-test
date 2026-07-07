# MSP Unity SDK (monorepo)

All Unity-related MSP code lives under this directory.

## Layout

```
msp-unity-sdk/
├── upm/                      # ai.themsp.unity.core (required)
├── packages/
│   └── adapter-nova/         # ai.themsp.unity.adapter.nova (optional)
├── android-bridge/           # Gradle project → msp-unity-bridge-release.aar
├── demo/                     # Unity demo project (core + nova)
├── docs/                     # publishing/integration docs
├── tools/release/            # release validation scripts
└── README.md
```

## Quick start

### 1. Open demo in Unity

Open `demo/` as a Unity project (Unity 6000.x tested).

The demo references local packages via `Packages/manifest.json`:

```json
"ai.themsp.unity.core": "file:../../upm",
"ai.themsp.unity.adapter.nova": "file:../../packages/adapter-nova"
```

See `docs/publishing-layout.md` for external release layout.

### 2. Build Android bridge AAR

For monorepo demo development:

```bash
cd android-bridge
make install
```

For release packaging (copies AAR into the publishable core package):

```bash
./tools/release/build-packages.sh
```

The release AAR is shipped inside `upm/Plugins/Android/msp-unity-bridge-release.aar`.
Packaged `.tgz` files are written to `build/` via `tools/release/build-packages.sh`.

### 3. Android build

Android native deps resolve from **Maven Central** (`ai.themsp:*`).

See `demo/Assets/Plugins/Android/README_MSP_SETUP.md` for Gradle / AdMob setup.

### 4. iOS build (CocoaPods)

The Unity iOS postprocess generates a `Podfile` from enabled adapters and runs
`pod install` automatically. By default it uses **CocoaPods trunk** (no local `msp-ios-sdk` required).

- Default iOS pods: `MSPCore`, `MSPNovaAdapter` at version `4.0.9`
- Skip auto pod install: set env `MSP_UNITY_SKIP_POD_INSTALL=1`
- Local SDK dev override: set env `MSP_UNITY_USE_LOCAL_IOS_SDK=1` (optional `MSP_IOS_SDK_PATH` for non-sibling `msp-ios-sdk`)
- `MSP_IOS_SDK_PATH` alone only affects CocoaPods Gemfile lookup during `pod install`

Build steps:

1. In Unity, switch target to iOS and **Build** (export Xcode project).
2. Open generated `Unity-iPhone.xcworkspace`.
3. Run on device from Xcode.

## Related repos (siblings under NewsBreak)

- `../msp-android` — native Android MSP SDK (Maven / local AAR source)
- `../msp-ios-sdk` — native iOS MSP SDK

## UPM package

The publishable Unity core plugin is in `upm/` (`ai.themsp.unity.core`).
Optional adapters live under `packages/` (Nova first).
See `docs/publishing-layout.md` and `upm/README.md` for API scope and status.
