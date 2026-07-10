#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.Applovin.Editor
{
    [InitializeOnLoad]
    internal static class ApplovinAdapterEditorRegistration
    {
        static ApplovinAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new ApplovinAdapterContributor().CreateDefinition());
        }
    }
}
#endif
