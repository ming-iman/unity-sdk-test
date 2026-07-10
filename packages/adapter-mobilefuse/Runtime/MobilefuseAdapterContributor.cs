using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Mobilefuse
{
    public sealed class MobilefuseAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "mobilefuse";
        public const string IosBootstrapClass = "MSPUnityMobilefuseBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "MobileFuse",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:mobilefuse-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MobilefuseAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
