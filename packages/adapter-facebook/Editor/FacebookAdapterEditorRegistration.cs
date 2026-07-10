#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Facebook.Editor
{
    [InitializeOnLoad]
    internal static class FacebookAdapterEditorRegistration
    {
        static FacebookAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new FacebookAdapterContributor().CreateDefinition());
        }
    }
}
#endif
