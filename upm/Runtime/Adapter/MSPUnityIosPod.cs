namespace MSP.Unity.Adapter
{
    public enum MSPUnityIosPodSource
    {
        SdkRoot,
        SdkThirdParty,
        Version
    }

    public readonly struct MSPUnityIosPod
    {
        public string Name { get; }
        public MSPUnityIosPodSource Source { get; }
        public string Version { get; }
        public string RelativePath { get; }

        public MSPUnityIosPod(string name, MSPUnityIosPodSource source, string version = null, string relativePath = null)
        {
            Name = name;
            Source = source;
            Version = version;
            RelativePath = relativePath;
        }
    }
}
