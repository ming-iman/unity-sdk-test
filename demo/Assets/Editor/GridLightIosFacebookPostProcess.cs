using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

/// <summary>
/// Adds Facebook Audience Network SKAdNetwork entries required for mediated test builds.
/// </summary>
public static class GridLightIosFacebookPostProcess
{
    private const string MarkerKey = "GridLightFacebookSkAdNetwork";

    [PostProcessBuild(1600)]
    private static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
        {
            return;
        }

        var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        if (!File.Exists(plistPath))
        {
            return;
        }

        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        var root = plist.root;

        if (root.values.ContainsKey(MarkerKey))
        {
            return;
        }

        root.SetString(MarkerKey, "1");

        var skArray = root.values.ContainsKey("SKAdNetworkItems")
            ? root["SKAdNetworkItems"].AsArray()
            : root.CreateArray("SKAdNetworkItems");

        AddSkAdNetwork(skArray, "v9wttpbfk9.skadnetwork");
        AddSkAdNetwork(skArray, "n38lu8286q.skadnetwork");

        File.WriteAllText(plistPath, plist.WriteToString());
        Debug.Log("[GridLight iOS] Added Facebook SKAdNetwork identifiers to Info.plist.");
    }

    private static void AddSkAdNetwork(PlistElementArray array, string identifier)
    {
        foreach (var entry in array.values)
        {
            if (entry is PlistElementDict dict &&
                dict.values.TryGetValue("SKAdNetworkIdentifier", out var existing) &&
                existing.AsString() == identifier)
            {
                return;
            }
        }

        var item = array.AddDict();
        item.SetString("SKAdNetworkIdentifier", identifier);
    }
}
