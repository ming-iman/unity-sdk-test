#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Mobilefuse.Editor
{
    [InitializeOnLoad]
    internal static class MobilefuseAdapterEditorRegistration
    {
        static MobilefuseAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new MobilefuseAdapterContributor().CreateDefinition());
        }
    }
}
#endif
