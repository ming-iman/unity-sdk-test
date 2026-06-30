#if UNITY_EDITOR
using UnityEditor;

namespace MSP.Unity.Editor
{
    internal static class MSPSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/MSP Unity SDK", SettingsScope.Project)
            {
                label = "MSP Unity SDK",
                guiHandler = _ =>
                {
                    EditorGUILayout.HelpBox(
                        "Interstitial MVP only. Native dependency integration (EDM4U/Pods) will be added in the next phase.",
                        MessageType.Info
                    );
                }
            };
        }
    }
}
#endif
