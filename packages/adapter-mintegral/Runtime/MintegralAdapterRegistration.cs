using MSP.Unity.Adapter;
using UnityEngine;

namespace MSP.Unity.Adapter.Mintegral
{
    public static class MintegralAdapterRegistration
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
            MSPUnityAdapterRegistry.Register(new MintegralAdapterContributor().CreateDefinition());
        }
    }
}
