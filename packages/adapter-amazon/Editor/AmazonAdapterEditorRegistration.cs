#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Amazon.Editor
{
    [InitializeOnLoad]
    internal static class AmazonAdapterEditorRegistration
    {
        static AmazonAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new AmazonAdapterContributor().CreateDefinition());
        }
    }
}
#endif
