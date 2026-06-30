using UnityEngine;

namespace MSP.Unity.Internal
{
    internal static class MSPClientFactory
    {
        public static IMSPClient Create()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidMSPClient();
#elif UNITY_IOS && !UNITY_EDITOR
            return new IOSMSPClient();
#else
            return new EditorMockMSPClient();
#endif
        }
    }
}
