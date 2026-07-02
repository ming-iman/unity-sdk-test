using System;
using MSP.Unity.Internal;

namespace MSP.Unity
{
    public static class MSP
    {
        private static readonly IMSPClient Client = MSPClientFactory.Create();

        public static string Version => Client.Version;

        public static void SetLogLevel(int level)
        {
            Client.SetLogLevel(level);
        }

        public static void Initialize(MSPInitializationParameters initParams, Action<bool, string> onComplete = null)
        {
            Client.Initialize(initParams, onComplete);
        }
    }
}
