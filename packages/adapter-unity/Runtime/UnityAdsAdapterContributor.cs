using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.UnityAds
{
    public sealed class UnityAdsAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "unity";
        public const string IosBootstrapClass = "MSPUnityUnityAdsBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Unity Ads",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:unity-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("UnityAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
