using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MSP.Unity
{
    /// <summary>
    /// Ad load request. Mirrors native MSP <c>AdRequest</c>:
    /// custom targeting via <see cref="CustomParams"/> and test/debug via <see cref="TestParams"/>.
    /// Prefer putting <c>ad_network</c> / <c>test_ad</c> in <see cref="TestParams"/> (same as Android/iOS demos).
    /// </summary>
    public sealed class MSPAdRequest
    {
        public MSPAdRequest(
            string placementId,
            IDictionary<string, object> customParams = null,
            IDictionary<string, object> testParams = null)
        {
            PlacementId = placementId ?? throw new ArgumentNullException(nameof(placementId));
            if (customParams != null)
            {
                foreach (var pair in customParams)
                {
                    CustomParams[pair.Key] = pair.Value;
                }
            }

            if (testParams != null)
            {
                foreach (var pair in testParams)
                {
                    TestParams[pair.Key] = pair.Value;
                }
            }
        }

        public string PlacementId { get; }

        /// <summary>Custom targeting / request params forwarded to native MSP.</summary>
        public Dictionary<string, object> CustomParams { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Test / debug params (e.g. <c>test_ad</c>, <c>ad_network</c>).
        /// Empty means a normal (non-forced-network) request.
        /// </summary>
        public Dictionary<string, object> TestParams { get; } = new Dictionary<string, object>();
    }
}
