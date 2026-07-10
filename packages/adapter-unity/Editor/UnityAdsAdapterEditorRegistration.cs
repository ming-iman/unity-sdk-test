#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Adapter.UnityAds.Editor
{
    [InitializeOnLoad]
    internal static class UnityAdsAdapterEditorRegistration
    {
        static UnityAdsAdapterEditorRegistration()
        {
            MSPUnityAdapterRegistry.Register(new UnityAdsAdapterContributor().CreateDefinition());
        }
    }
}
#endif
