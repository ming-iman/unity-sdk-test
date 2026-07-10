#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Pubmatic.Editor
{
    [InitializeOnLoad]
    internal static class PubmaticAdapterEditorRegistration
    {
        static PubmaticAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new PubmaticAdapterContributor().CreateDefinition());
        }
    }
}
#endif
