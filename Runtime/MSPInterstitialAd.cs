using MSP.Unity.Internal;

namespace MSP.Unity
{
    public sealed class MSPInterstitialAd : MSPAd
    {
        private readonly IMSPClient client;
        private readonly MSPAdListener adListener;

        internal MSPInterstitialAd(string placementId, IMSPClient client, MSPAdListener adListener) : base(placementId)
        {
            this.client = client;
            this.adListener = adListener;
        }

        public void Show()
        {
            client.ShowAd(PlacementId, NativeAdToken);
            if (adListener != null && adListener.OnAdImpression != null)
            {
                adListener.OnAdImpression(this);
            }
            if (adListener != null && adListener.OnAdDismissed != null)
            {
                adListener.OnAdDismissed(this);
            }
        }
    }
}
