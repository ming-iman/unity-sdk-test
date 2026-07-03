#if UNITY_EDITOR
using MSP.Unity.Adapter;
using UnityEditor;

namespace MSP.Unity.Editor
{
    [InitializeOnLoad]
    internal static class MSPUnityAdapterEditorBootstrap
    {
        static MSPUnityAdapterEditorBootstrap()
        {
            MSPUnityAdapterRegistry.EnsureDiscovered();
        }
    }
}
#endif
