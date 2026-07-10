#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Liftoff.Editor
{
    [InitializeOnLoad]
    internal static class LiftoffAdapterEditorRegistration
    {
        static LiftoffAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new LiftoffAdapterContributor().CreateDefinition());
        }
    }
}
#endif
