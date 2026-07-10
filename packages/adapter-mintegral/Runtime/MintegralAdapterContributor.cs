using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Mintegral
{
    public sealed class MintegralAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "mintegral";
        public const string IosBootstrapClass = "MSPUnityMintegralBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Mintegral",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:mintegral-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MintegralAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
