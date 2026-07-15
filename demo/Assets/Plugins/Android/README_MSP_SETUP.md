# MSP Android Bridge Setup

This folder contains the Unity Android bridge AAR:

- `msp-unity-bridge-release.aar`

`network_security_config.xml` (for HTTPS packet capture) is packaged inside the bridge AAR.
Rebuild the AAR with `make install` in `android-bridge/` after bridge changes.
Do **not** add `Assets/Plugins/Android/res/` — Unity 6000+ rejects plugin resources outside AARs.

To run real ads on Android device, you still need runtime dependencies required by MSP.

Dependencies are declared by UPM packages (not in this demo `Assets` folder):

- Core (`ai.themsp.unity.core`): `msp-core`, `prebid-adapter` from Maven Central
- Nova adapter (`ai.themsp.unity.adapter.nova`): `nova-adapter` from Maven Central

After changing packages, run **Assets > External Dependency Manager > Android Resolver > Force Resolve**.

## Dependency resolution

Use EDM4U and enable **Custom Gradle Settings Template** (`Assets/Plugins/Android/settingsTemplate.gradle`).

Dependency XML files are provided by installed MSP UPM packages under `Packages/`.

External users only need Maven Central and Google Maven.

If app crashes with `ClassNotFoundException` or `NoClassDefFoundError`, a runtime dependency is missing.

## AdMob App ID (required for google-adapter)

`google-adapter` pulls in Google Mobile Ads, which requires an AdMob App ID manifest entry.

**Important:** `Assets/Plugins/Android/AndroidManifest.xml` is a **custom library manifest template**.
It must include Unity's launcher activity (`UnityPlayerGameActivity` with `MAIN`/`LAUNCHER`),
not just the AdMob `meta-data`. If the launcher activity is missing, Unity cannot auto-start the app after install.

This demo's manifest is based on Unity's `UnityManifest.xml` with the Google **sample** AdMob App ID added.

## Bundle ID

Set your own application id in **Player Settings → Android → Package Name** (this demo uses `com.particlemedia.msp`).
