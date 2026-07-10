#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Moloco.Editor
{
    [InitializeOnLoad]
    internal static class MolocoAdapterEditorRegistration
    {
        static MolocoAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new MolocoAdapterContributor().CreateDefinition());
        }
    }
}
#endif
