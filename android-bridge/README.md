# MSP Unity Android Bridge AAR

This project builds the Android bridge AAR for Unity:

- Module: `:bridge`
- Output: `bridge/build/outputs/aar/msp-unity-bridge-release.aar`

## Build

From this directory:

```bash
make install
```

Or build/copy separately:

```bash
make build
make copy
```

Requires a local `./gradlew` or `gradle` on `PATH`. Override if needed:

```bash
GRADLEW=/path/to/gradlew make install
UNITY_PLUGINS_DIR=/path/to/YourUnityProject/Assets/Plugins/Android make install
```

For release packaging, copy into the publishable core package:

```bash
UNITY_PLUGINS_DIR=../upm/Plugins/Android make install
```

Or run `./tools/release/build-packages.sh` from the repo root.

Manual Gradle (equivalent to `make build`):

```bash
gradle -p . :bridge:assembleRelease
# or: ./gradlew -p . :bridge:assembleRelease
```

## Copy to Unity project

`make copy` places the AAR at:

`../demo/Assets/Plugins/Android/msp-unity-bridge-release.aar`

For other Unity projects, set `UNITY_PLUGINS_DIR`.

The AAR also packages `res/xml/network_security_config.xml` (user CA trust for HTTPS debugging).
Reference it from your Unity `AndroidManifest.xml` with
`android:networkSecurityConfig="@xml/network_security_config"`.
Do not put `Assets/Plugins/Android/res/` in Unity 6000+.
