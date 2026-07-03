namespace MSP.Unity.Adapter
{
    /// <summary>
    /// Public native SDK versions for external distribution.
    /// Android artifacts are on Maven Central (group: ai.themsp).
    /// iOS pods are on CocoaPods trunk.
    /// </summary>
    public static class MSPUnityNativeVersions
    {
        public const string AndroidMavenVersion = "4.0.0";
        public const string IosPodVersion = "4.0.9";
        public const string SwiftProtobufPodVersion = "~> 1.28.2";
    }
}
