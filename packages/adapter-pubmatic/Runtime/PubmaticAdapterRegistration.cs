using MSP.Unity.Adapter;
using UnityEngine;

namespace MSP.Unity.Adapter.Pubmatic
{
    public static class PubmaticAdapterRegistration
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
            MSPUnityAdapterRegistry.Register(new PubmaticAdapterContributor().CreateDefinition());
        }
    }
}
