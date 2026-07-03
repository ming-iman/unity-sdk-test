using System.Collections.Generic;

namespace MSP.Unity.Adapter
{
    public sealed class MSPUnityAdapterDefinition
    {
        public string AdapterId { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> AndroidMavenSpecs { get; }
        public IReadOnlyList<MSPUnityIosPod> IosPods { get; }
        public string IosBootstrapClassName { get; }
        public bool RequiresGoogleAdsAppId { get; }

        public MSPUnityAdapterDefinition(
            string adapterId,
            string displayName,
            IReadOnlyList<string> androidMavenSpecs,
            IReadOnlyList<MSPUnityIosPod> iosPods,
            string iosBootstrapClassName,
            bool requiresGoogleAdsAppId = false)
        {
            AdapterId = adapterId;
            DisplayName = displayName;
            AndroidMavenSpecs = androidMavenSpecs ?? new string[0];
            IosPods = iosPods ?? new MSPUnityIosPod[0];
            IosBootstrapClassName = iosBootstrapClassName;
            RequiresGoogleAdsAppId = requiresGoogleAdsAppId;
        }
    }
}
