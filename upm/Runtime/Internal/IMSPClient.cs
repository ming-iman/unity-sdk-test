using System;
using MSP.Unity;

namespace MSP.Unity.Internal
{
    internal interface IMSPClient
    {
        string Version { get; }
        void SetLogLevel(int level);
        void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete);
        string CreateAdLoader();
        void DestroyAdLoader(string loaderId);
        void LoadAd(string loaderId, string placementId, MSPAdRequest adRequest, MSPAdListener adListener);
        MSPAd GetAd(string loaderId, string placementId, MSPAdListener adListener);
        void ShowAd(string loaderId);
    }
}
