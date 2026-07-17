using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// iOS MSP bridge clears native AdListener on first load error (auction timeout),
/// but Prebid can still deliver the winning bid afterward. MSPBidder then logs
/// "[Auction] Ads no filled. Reason: Invalid request" because its weak adListener is nil.
/// This patch keeps the native listener alive until dismiss/success cleanup.
/// </summary>
public static class GridLightMspListenerHoldFix
{
    private const string SwiftMarker = "[GridLight] keep listener on load error";
    private const string CSharpMarker = "[GridLight] keep listener on load error";

    [InitializeOnLoadMethod]
    private static void PatchManagedBridgeOnLoad()
    {
        PatchManagedBridge(FindManagedBridgePath());
    }

    public class PostBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;
            PatchIosBridge(Path.Combine(report.summary.outputPath, "Libraries"));
        }
    }

    private static void PatchIosBridge(string librariesRoot)
    {
        if (string.IsNullOrEmpty(librariesRoot) || !Directory.Exists(librariesRoot)) return;

        string swiftPath = null;
        foreach (var path in Directory.GetFiles(librariesRoot, "MSPUnityEntry.swift", SearchOption.AllDirectories))
        {
            swiftPath = path;
            break;
        }

        if (string.IsNullOrEmpty(swiftPath))
        {
            Debug.LogWarning("[GridLight MSP] MSPUnityEntry.swift not found in iOS export; skip listener hold patch.");
            return;
        }

        var text = File.ReadAllText(swiftPath);
        if (text.Contains(SwiftMarker))
        {
            Debug.Log("[GridLight MSP] iOS listener hold patch already applied.");
            return;
        }

        const string oldBlock = "        MSPUnityEntry.clearAdState(for: requestToken)\n    }\n\n    func onAdRewardReceived";
        const string newBlock =
            "        // " + SwiftMarker + "\n" +
            "        // Keep native AdListener alive so late bid responses can still route to Facebook/Google.\n" +
            "    }\n\n    func onAdRewardReceived";

        if (!text.Contains(oldBlock))
        {
            Debug.LogWarning("[GridLight MSP] MSPUnityEntry.swift onError anchor not found; skip listener hold patch.");
            return;
        }

        text = text.Replace(oldBlock, newBlock);
        File.WriteAllText(swiftPath, text);
        Debug.Log($"[GridLight MSP] Applied iOS listener hold patch to {swiftPath}");
    }

    private static void PatchManagedBridge(string listenerPath)
    {
        if (string.IsNullOrEmpty(listenerPath) || !File.Exists(listenerPath)) return;

        var text = File.ReadAllText(listenerPath);
        if (text.Contains(CSharpMarker))
        {
            return;
        }

        var pattern =
            @"listener\.OnError\(message\.error \?\? ""Unknown error"", loadInfo\);\s*\r?\n\s*UnregisterLoadListener\(message\.requestToken, message\.placementId\);";
        var replacement =
            "listener.OnError(message.error ?? \"Unknown error\", loadInfo);\n            // " + CSharpMarker +
            "\n            // Do not unregister yet; late native callbacks may still arrive after auction timeout.";

        var updated = Regex.Replace(text, pattern, replacement);
        if (updated == text)
        {
            Debug.LogWarning("[GridLight MSP] MSPUnityListener.cs OnNativeError anchor not found; skip managed patch.");
            return;
        }

        File.WriteAllText(listenerPath, updated);
        Debug.Log($"[GridLight MSP] Applied managed listener hold patch to {listenerPath}");
    }

    private static string FindManagedBridgePath()
    {
        var packageRoot = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
        if (!Directory.Exists(packageRoot)) return null;

        foreach (var dir in Directory.GetDirectories(packageRoot, "ai.themsp.unity.core@*"))
        {
            var candidate = Path.Combine(dir, "Runtime", "Internal", "MSPUnityListener.cs");
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }
}
