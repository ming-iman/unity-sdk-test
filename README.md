# MSP Unity SDK (monorepo)

All Unity-related MSP code lives under this directory.

## Layout

```
msp-unity-sdk/
├── upm/              # Unity Package Manager plugin (ai.themsp.unity)
├── android-bridge/   # Gradle project → msp-unity-bridge-release.aar
├── demo/             # Unity demo project (Android Interstitial test)
└── README.md
```

## Quick start

### 1. Open demo in Unity

Open `demo/` as a Unity project (Unity 6000.x tested).

The demo references the local package via `Packages/manifest.json`:

```json
"ai.themsp.unity": "file:../../upm"
```

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

The publishable Unity plugin is in `upm/`. See `upm/README.md` for API scope and status.
