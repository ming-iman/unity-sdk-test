using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Applovin
{
    public sealed class ApplovinAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "applovin";
        public const string IosBootstrapClass = "MSPUnityApplovinBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "AppLovin",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:applovin-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPApplovinMaxAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
