using System;
using System.Collections.Generic;

namespace MSP.Unity
{
    public sealed class MSPAdRequest
    {
        public MSPAdRequest(string placementId, string adNetwork = null)
        {
            PlacementId = placementId ?? throw new ArgumentNullException(nameof(placementId));
            AdNetwork = adNetwork;
        }

        public string PlacementId { get; }
        public string AdNetwork { get; set; }
        public Dictionary<string, object> CustomParams { get; } = new Dictionary<string, object>();
    }
}
