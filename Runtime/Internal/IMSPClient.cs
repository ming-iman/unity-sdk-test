using System;
using MSP.Unity;

namespace MSP.Unity.Internal
{
    internal interface IMSPClient
    {
        string Version { get; }
        void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete);
        void LoadInterstitial(string placementId, MSPAdRequest adRequest, MSPAdListener adListener, Action<string> cacheAdToken);
        void ShowInterstitial(string placementId, string nativeAdToken);
    }
}
