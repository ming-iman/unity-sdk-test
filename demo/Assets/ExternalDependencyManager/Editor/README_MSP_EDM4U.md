MSP runtime dependency setup for Unity (Android)

1) Install EDM4U in this Unity project:
- Package Manager -> Add package from git URL
- https://github.com/googlesamples/unity-jar-resolver.git?path=upm

2) Resolve Android dependencies:
- Assets -> External Dependency Manager -> Android Resolver -> Force Resolve

3) Keep bridge AAR in:
- Assets/Plugins/Android/msp-unity-bridge-release.aar

Android native MSP artifacts resolve from **Maven Central** (`ai.themsp:*`) and Google Maven. No private repository credentials are required.
