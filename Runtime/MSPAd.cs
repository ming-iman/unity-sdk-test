namespace MSP.Unity
{
    public abstract class MSPAd
    {
        protected MSPAd(string placementId)
        {
            PlacementId = placementId;
        }

        public string PlacementId { get; }
        internal string NativeAdToken { get; set; } = string.Empty;
    }
}
