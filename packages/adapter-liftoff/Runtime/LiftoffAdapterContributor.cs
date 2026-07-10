using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Liftoff
{
    public sealed class LiftoffAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "liftoff";
        public const string IosBootstrapClass = "MSPUnityLiftoffBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Liftoff",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:liftoff-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPLiftoffAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
