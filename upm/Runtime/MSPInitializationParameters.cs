using System.Collections.Generic;

namespace MSP.Unity
{
    public sealed class MSPInitializationParameters
    {
        public string PrebidApiKey { get; set; } = string.Empty;
        public string SourceApp { get; set; } = string.Empty;
        public long OrgId { get; set; }
        public long AppId { get; set; }
        public string PrebidHost { get; set; } = string.Empty;
        public bool HasUserConsent { get; set; } = true;
        public bool IsAgeRestrictedUser { get; set; }
        public bool IsDoNotSell { get; set; }
        public bool IsInTestMode { get; set; }
        public string ConsentString { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string AppPackageName { get; set; } = string.Empty;
        public string AppVersionName { get; set; } = string.Empty;
    }
}
