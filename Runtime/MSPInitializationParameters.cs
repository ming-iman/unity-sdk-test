using System.Collections.Generic;

namespace MSP.Unity
{
    public sealed class MSPInitializationParameters
    {
        public string PrebidApiKey { get; set; } = string.Empty;
        public int OrgId { get; set; }
        public int AppId { get; set; }
        public bool HasUserConsent { get; set; } = true;
        public bool IsAgeRestrictedUser { get; set; }
        public bool IsDoNotSell { get; set; }
        public bool IsInTestMode { get; set; }
        public string ConsentString { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
