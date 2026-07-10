using System.Collections.Generic;
using MSP.Unity.Adapter;

namespace MSP.Unity.Adapter.Facebook
{
    public sealed class FacebookAdapterContributor : IMSPUnityAdapterContributor
    {
        public const string AdapterIdValue = "facebook";
        public const string IosBootstrapClass = "MSPUnityFacebookBootstrap";

        public MSPUnityAdapterDefinition CreateDefinition()
        {
            return new MSPUnityAdapterDefinition(
                adapterId: AdapterIdValue,
                displayName: "Facebook",
                androidMavenSpecs: new[]
                {
                    $"ai.themsp:facebook-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
                },
                iosPods: new[]
                {
                    new MSPUnityIosPod("MSPFacebookAdapter", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion)
                },
                iosBootstrapClassName: IosBootstrapClass,
                requiresGoogleAdsAppId: false);
        }
    }
}
