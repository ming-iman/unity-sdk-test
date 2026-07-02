MSP runtime dependency setup for Unity (Android)

1) Install EDM4U in this Unity project:
- Package Manager -> Add package from git URL
- https://github.com/googlesamples/unity-jar-resolver.git?path=upm

2) Resolve Android dependencies:
- Assets -> External Dependency Manager -> Android Resolver -> Force Resolve

3) Keep bridge AAR in:
- Assets/Plugins/Android/msp-unity-bridge-release.aar

If resolve fails on private artifacts, ensure network can reach:
- https://artifactory.nb-sandbox.com/artifactory/libs-release-local
- https://artifactory.nb-sandbox.com/artifactory/libs-snapshot-local
