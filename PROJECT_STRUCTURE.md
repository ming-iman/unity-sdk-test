# MSP Unity SDK Directory Structure

This file explains the purpose of subdirectories inside `./msp-unity-sdk` (the Unity SDK project itself).

## Subdirectories

- `android-bridge`: Native Android bridge code and Gradle modules used by the Unity plugin.
- `demo`: Unity demo project used for SDK integration testing and validation.
- `docs`: Project documentation (integration notes, publishing guides, design notes).
- `packages`: Optional Unity packages (for example, adapter-specific packages such as Nova).
- `tools`: Development and release helper scripts.
- `upm`: Core Unity UPM package for MSP SDK runtime/editor functionality.

## Second-Level Directories

- `android-bridge/bridge`: Android library module that exposes MSP native APIs to Unity.
- `android-bridge/gradle`: Shared Gradle wrapper/config files for Android bridge builds.

- `demo/Assets`: Unity assets, scenes, scripts, and plugin configuration used by the demo app.
- `demo/Packages`: Unity package manifest and lock files for demo dependency resolution.
- `demo/ProjectSettings`: Unity project-level settings for build/runtime behavior.

- `docs/*`: Integration and publishing docs; includes setup guides and release notes.

- `packages/adapter-nova`: Optional Nova adapter Unity package (runtime + editor pieces).
- `packages/*/Runtime`: Runtime integration logic for optional packages.
- `packages/*/Editor`: Editor-time install/config/build helpers for optional packages.

- `tools/release`: Release and validation scripts for packaging/publishing.
- `tools/*`: Internal utility scripts used in local development and CI.
- `build/`: Generated release artifacts (`.tgz`); gitignored.

- `upm/Runtime`: Main runtime APIs used by Unity apps at run time.
- `upm/Editor`: Unity editor automation (post-process, dependency generation, iOS/Android integration).
- `upm/Plugins`: Native plugin bridge sources and platform-specific interop code.
  - `upm/Plugins/Android`: Shipped `msp-unity-bridge-release.aar` for Unity ↔ Android Java bridge.
  - `upm/Plugins/iOS`: Swift/ObjC bridge (`MSPUnityEntry`, `MSPUnityBridge.mm`).

## Notes

- The `upm` folder is the main package consumed by integrators.
- The `packages` folder contains optional add-ons that can be installed on top of `upm`.
- The `demo` project is the fastest way to verify end-to-end SDK behavior.

