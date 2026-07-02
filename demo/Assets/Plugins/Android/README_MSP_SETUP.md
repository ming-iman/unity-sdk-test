# MSP Android Bridge Setup

This folder contains the Unity Android bridge AAR:

- `msp-unity-bridge-release.aar`

`network_security_config.xml` (for HTTPS packet capture) is packaged inside the bridge AAR.
Rebuild the AAR with `make install` in `msp-unity-android-bridge` after bridge changes.
Do **not** add `Assets/Plugins/Android/res/` — Unity 6000+ rejects plugin resources outside AARs.

To run real ads on Android device, you still need runtime dependencies required by MSP:

- `ai.themsp:prebid-adapter:4.1.0`
- `ai.themsp:msp-core:4.1.0`
- `ai.themsp:google-adapter:4.1.0` (required for `ad_network=msp_google` test ads)
- `ai.themsp:nova-adapter:4.1.0` (required for `ad_network=msp_nova` test ads)
- Their transitive dependencies (Google Play services, Kotlin stdlib, etc.)

## Dependency resolution

Use EDM4U (`Assets/ExternalDependencyManager/Editor/MSPDependencies.xml`) and enable
**Custom Gradle Settings Template** (`Assets/Plugins/Android/settingsTemplate.gradle`).

The private Artifactory repos require credentials. `settingsTemplate.gradle` includes the
same `services` / `services` credentials used by `msp-android/settings.gradle`.

If Gradle fails with `401 Unauthorized` from `artifactory.nb-sandbox.com`, confirm
`settingsTemplate.gradle` has the `credentials { }` block on both Artifactory `maven` entries.

Alternative for offline builds: build local AARs from `msp-android` (`assembleLocalRelease`)
and copy them here, then remove the Maven `implementation` lines from `mainTemplate.gradle`.

If app crashes with `ClassNotFoundException` or `NoClassDefFoundError`, a runtime dependency is missing.

## AdMob App ID (required for google-adapter)

`google-adapter` pulls in Google Mobile Ads, which requires this manifest entry (same as `msp-android` demo).

**Important:** `Assets/Plugins/Android/AndroidManifest.xml` is a **custom library manifest template**.
It must include Unity's launcher activity (`UnityPlayerGameActivity` with `MAIN`/`LAUNCHER`),
not just the AdMob `meta-data`. If the launcher activity is missing, Unity cannot auto-start the app after install.

This demo's manifest is based on Unity's `UnityManifest.xml` with the AdMob App ID added.

## Bundle ID

Use `com.particlemedia.msp` (same as `msp-android` demo) in **Player Settings → Android → Package Name**.
