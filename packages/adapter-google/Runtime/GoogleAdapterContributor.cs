using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Google
{
    public sealed class GoogleAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "google";
        public const string IosBootstrapClass = "MSPUnityGoogleBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Google",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:google-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPGoogleAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: true);
        }
    }
}
