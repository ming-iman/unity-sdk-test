using System;
using System.Collections.Generic;

namespace MSP.Unity
{
    public sealed class MSPAdListener
    {
        public Action<string, Dictionary<string, object>> OnAdLoaded { get; set; }
        public Action<string, Dictionary<string, object>> OnError { get; set; }
        public Action<MSPAd> OnAdImpression { get; set; }
        public Action<MSPAd> OnAdClick { get; set; }
        public Action<MSPAd> OnAdDismissed { get; set; }
    }
}
