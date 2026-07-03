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

```bash
cd android-bridge
make install
```

This builds the AAR and copies it to `demo/Assets/Plugins/Android/`.

### 3. Android build

See `demo/Assets/Plugins/Android/README_MSP_SETUP.md` for Gradle / Artifactory / AdMob setup.

### 4. iOS build (Pod-based)

The Unity iOS postprocess now generates a `Podfile` in exported Xcode projects and runs
`pod install` automatically.

- Default MSP iOS SDK path: `../msp-ios-sdk` (sibling of `msp-unity-sdk`)
- Override path: set env `MSP_IOS_SDK_PATH=/absolute/path/to/msp-ios-sdk`
- Skip auto pod install: set env `MSP_UNITY_SKIP_POD_INSTALL=1`

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
