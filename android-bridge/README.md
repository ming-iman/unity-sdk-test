# MSP Unity Android Bridge AAR

This project builds the Android bridge AAR for Unity:

- Module: `:bridge`
- Output: `bridge/build/outputs/aar/msp-unity-bridge-release.aar`

## Build

From this repo:

```bash
make install
```

Or build/copy separately:

```bash
make build
make copy
```

`make install` uses `../../msp-android/gradlew` by default. Override Unity destination if needed:

```bash
UNITY_PLUGINS_DIR=/path/to/YourUnityProject/Assets/Plugins/Android make install
```

For release packaging, copy into the publishable core package:

```bash
UNITY_PLUGINS_DIR=../upm/Plugins/Android make install
```

Or run `./tools/release/build-packages.sh` from the repo root.

Manual Gradle (equivalent to `make build`):

```bash
../../msp-android/gradlew -p . :bridge:assembleRelease
```

## Copy to Unity project

`make copy` places the AAR at:

`../demo/Assets/Plugins/Android/msp-unity-bridge-release.aar`

For other Unity projects, set `UNITY_PLUGINS_DIR`.

The AAR also packages `res/xml/network_security_config.xml` (user CA trust for HTTPS debugging).
Reference it from your Unity `AndroidManifest.xml` with
`android:networkSecurityConfig="@xml/network_security_config"`.
Do not put `Assets/Plugins/Android/res/` in Unity 6000+.
