using System.IO;
using System.Linq;
using UnityEngine;

namespace MSP.Unity.Editor
{
    internal static class MSPUnityIosPodsEmbedCleaner
    {
        private static readonly string[] BlockedFrameworkNames =
        {
            "MSPSnapKit",
            "MSPKingfisher",
            "MSPNovaAdapter"
        };

        internal static void StripBlockedFrameworks(string xcodeProjectPath)
        {
            var podsSupportDir = Path.Combine(
                xcodeProjectPath,
                "Pods",
                "Target Support Files",
                "Pods-UnityFramework");
            if (!Directory.Exists(podsSupportDir))
            {
                return;
            }

            var changed = 0;
            foreach (var file in Directory.GetFiles(podsSupportDir))
            {
                var fileName = Path.GetFileName(file);
                if (!fileName.Contains("frameworks"))
                {
                    continue;
                }

                if (StripBlockedLines(file))
                {
                    changed++;
                }
            }

            if (changed > 0)
            {
                Debug.Log($"[MSP iOS] Removed blocked local-only frameworks from {changed} CocoaPods embed script(s).");
            }
        }

        private static bool StripBlockedLines(string path)
        {
            var lines = File.ReadAllLines(path);
            var filtered = lines
                .Where(line => BlockedFrameworkNames.All(blocked => !line.Contains(blocked)))
                .ToArray();
            if (filtered.Length == lines.Length)
            {
                return false;
            }

            File.WriteAllLines(path, filtered);
            return true;
        }
    }
}
