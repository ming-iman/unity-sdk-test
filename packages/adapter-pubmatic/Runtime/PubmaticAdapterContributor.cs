using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Pubmatic
{
    public sealed class PubmaticAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "pubmatic";
        public const string IosBootstrapClass = "MSPUnityPubmaticBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "PubMatic",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:pubmatic-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("PubmaticAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
