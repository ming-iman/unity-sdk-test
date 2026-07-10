using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Moloco
{
    public sealed class MolocoAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "moloco";
        public const string IosBootstrapClass = "MSPUnityMolocoBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Moloco",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:moloco-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPMolocoAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
