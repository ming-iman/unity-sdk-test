using UnityEngine;

public static class GridLightTheme
{
    public const string LevelPrefsKey = "gridlight_current_level";

    public static readonly Color32 BgWarm = new Color32(255, 241, 220, 255);
    public static readonly Color32 BgGameplay = new Color32(236, 242, 246, 255);
    public static readonly Color32 Panel = new Color32(255, 248, 236, 255);
    public static readonly Color32 PanelDeep = new Color32(242, 214, 178, 255);
    public static readonly Color32 Accent = new Color32(236, 138, 48, 255);
    public static readonly Color32 AccentSoft = new Color32(255, 196, 110, 255);
    public static readonly Color32 Success = new Color32(104, 176, 98, 255);
    public static readonly Color32 TextDark = new Color32(62, 42, 30, 255);
    public static readonly Color32 TextMuted = new Color32(120, 90, 68, 255);
    public static readonly Color32 Overlay = new Color32(48, 32, 20, 150);
    public static readonly Color32 PanelStroke = new Color32(210, 168, 120, 90);

    // Cool slate floors so warm lamps read clearly from top-down.
    public static readonly Color32 FloorUnlit = new Color32(92, 118, 132, 255);
    public static readonly Color32 FloorUnlitEmission = new Color32(48, 68, 78, 255);
    public static readonly Color32 FloorLit = new Color32(242, 210, 132, 255);
    public static readonly Color32 FloorLitEmission = new Color32(255, 196, 96, 255);
    public static readonly Color32 FloorBase = new Color32(58, 74, 86, 255);

    public static void ApplySceneLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color32(188, 200, 210, 255);
        RenderSettings.fog = false;

        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.type != LightType.Directional) continue;
            light.intensity = 0.7f;
            light.color = new Color(0.92f, 0.95f, 1f);
            light.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            light.shadows = LightShadows.None;
        }
    }
}
