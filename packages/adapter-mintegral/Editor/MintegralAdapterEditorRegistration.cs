#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Mintegral.Editor
{
    [InitializeOnLoad]
    internal static class MintegralAdapterEditorRegistration
    {
        static MintegralAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new MintegralAdapterContributor().CreateDefinition());
        }
    }
}
#endif
