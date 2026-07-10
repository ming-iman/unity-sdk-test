using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Inmobi
{
    public sealed class InmobiAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "inmobi";
        public const string IosBootstrapClass = "MSPUnityInmobiBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "InMobi",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:inmobi-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("InmobiAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
