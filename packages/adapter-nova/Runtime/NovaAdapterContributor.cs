using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Nova
{
    public sealed class NovaAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "nova";
        public const string IosBootstrapClass = "MSPUnityNovaBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Nova",
                androidMavenSpecs: new[]
                {
                    "ai.themsp:nova-adapter:4.1.0"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPNovaAdapter", MSPUnityIosPodSource.SdkRoot),
                    new MSPUnityIosPod("NovaCore", MSPUnityIosPodSource.SdkRoot),
                    new MSPUnityIosPod("MSPKingfisher", MSPUnityIosPodSource.SdkThirdParty, relativePath: "ThirdParty/MSPKingfisher"),
                    new MSPUnityIosPod("MSPSnapKit", MSPUnityIosPodSource.SdkThirdParty, relativePath: "ThirdParty/MSPSnapKit")
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
