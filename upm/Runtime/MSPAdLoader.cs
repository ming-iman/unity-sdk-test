using System;
using System.Collections.Generic;
using MSP.Unity.Internal;

namespace MSP.Unity
{
    public sealed class MSPAdLoader
    {
        private readonly IMSPClient client;
        private readonly Dictionary<string, MSPAdListener> listeners = new Dictionary<string, MSPAdListener>();

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

            listeners[placementId] = adListener;
            client.LoadAd(placementId, adRequest, adListener);
        }

        public MSPAd GetAd(string placementId)
        {
            if (!listeners.TryGetValue(placementId, out var adListener))
            {
                return null;
            }

            return client.GetAd(placementId, adListener);
        }
    }
}
