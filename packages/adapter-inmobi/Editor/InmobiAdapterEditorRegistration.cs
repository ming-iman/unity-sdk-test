#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Inmobi.Editor
{
    [InitializeOnLoad]
    internal static class InmobiAdapterEditorRegistration
    {
        static InmobiAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new InmobiAdapterContributor().CreateDefinition());
        }
    }
}
#endif
