using System;
using System.Collections.Generic;

namespace MSP.Unity.Internal
{
    [Serializable]
    internal sealed class NativeLoadInfo
    {
        public string request_id;
    }

    [Serializable]
    internal sealed class NativeLoadMessage
    {
        public string placementId;
        public string requestToken;
        public NativeLoadInfo loadInfo;
    }

    [Serializable]
    internal sealed class NativeErrorMessage
    {
        public string placementId;
        public string requestToken;
        public string error;
        public NativeLoadInfo loadInfo;
    }

    [Serializable]
    internal sealed class NativeEventMessage
    {
        public string placementId;
        public string requestToken;
        public string eventName;
    }

    internal static class NativeBridgeMessages
    {
        internal static Dictionary<string, object> ToLoadInfoDictionary(NativeLoadInfo loadInfo)
        {
            var dict = new Dictionary<string, object>();
            if (loadInfo != null && !string.IsNullOrEmpty(loadInfo.request_id))
            {
                dict["request_id"] = loadInfo.request_id;
            }
            return dict;
        }
    }
}
