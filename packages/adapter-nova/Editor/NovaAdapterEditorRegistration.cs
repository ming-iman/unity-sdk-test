#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Nova.Editor
{
    [InitializeOnLoad]
    internal static class NovaAdapterEditorRegistration
    {
        static NovaAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new NovaAdapterContributor().CreateDefinition());
        }
    }
}
#endif
