using System;
using System.Collections.Generic;

namespace MSP.Unity
{
    public sealed class MSPAdRequest
    {
        public MSPAdRequest(string placementId)
        {
            PlacementId = placementId ?? throw new ArgumentNullException(nameof(placementId));
        }

        public string PlacementId { get; }
        public Dictionary<string, object> CustomParams { get; } = new Dictionary<string, object>();
    }
}
