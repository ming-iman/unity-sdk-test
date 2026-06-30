using System;
using MSP.Unity;

namespace MSP.Unity.Internal
{
    internal interface IMSPClient
    {
        string Version { get; }
        void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete);
        void LoadAd(string placementId, MSPAdRequest adRequest, MSPAdListener adListener);
        MSPAd GetAd(string placementId, MSPAdListener adListener);
        void ShowAd(string placementId, string nativeAdToken);
    }
}
