using MSP.Unity.Adapter;
using UnityEngine;

namespace MSP.Unity.Adapter.Mobilefuse
{
    public static class MobilefuseAdapterRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterAfterAssembliesLoaded()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterBeforeSceneLoad()
        {
            Register();
        }

        public static void Register()
        {
            MSPUnityAdapterRegistry.Register(new MobilefuseAdapterContributor().CreateDefinition());
        }
    }
}
