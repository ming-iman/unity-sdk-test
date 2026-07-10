using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Amazon
{
    public sealed class AmazonAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "amazon";
        public const string IosBootstrapClass = "MSPUnityAmazonBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Amazon",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:amazon-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPAmazonAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
