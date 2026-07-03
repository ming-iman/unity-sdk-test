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
                    $"ai.themsp:nova-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPNovaAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
