namespace MSP.Unity
{
    public abstract class MSPAd
    {
        protected MSPAd(string placementId)
        {
            PlacementId = placementId;
        }

        public string PlacementId { get; }
        internal string LoaderId { get; set; } = string.Empty;
    }
}
