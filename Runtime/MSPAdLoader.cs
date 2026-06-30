using System;
using System.Collections.Generic;
using MSP.Unity.Internal;

namespace MSP.Unity
{
    public sealed class MSPAdLoader
    {
        private readonly IMSPClient client;
        private readonly Dictionary<string, MSPInterstitialAd> interstitialCache = new Dictionary<string, MSPInterstitialAd>();

        public MSPAdLoader()
        {
            client = MSPClientFactory.Create();
        }

        public void LoadAd(string placementId, MSPAdListener adListener, MSPAdRequest adRequest)
        {
            if (adRequest == null)
            {
                throw new ArgumentNullException(nameof(adRequest));
            }

            client.LoadInterstitial(placementId, adRequest, adListener, nativeToken =>
            {
                var ad = new MSPInterstitialAd(placementId, client, adListener)
                {
                    NativeAdToken = nativeToken
                };
                interstitialCache[placementId] = ad;
            });
        }

        public MSPInterstitialAd GetAd(string placementId)
        {
            interstitialCache.TryGetValue(placementId, out var ad);
            return ad;
        }
    }
}
