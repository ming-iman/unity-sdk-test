#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Google.Editor
{
    [InitializeOnLoad]
    internal static class GoogleAdapterEditorRegistration
    {
        static GoogleAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new GoogleAdapterContributor().CreateDefinition());
        }
    }
}
#endif
