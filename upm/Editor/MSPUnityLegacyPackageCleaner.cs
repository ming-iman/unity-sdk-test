using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MSP.Unity.Editor
{
    internal static class MSPUnityLegacyPackageCleaner
    {
        private static readonly Regex LegacyPathRegex = new Regex(
            @"Libraries/ai\.themsp\.unity/(?!core|adapter)",
            RegexOptions.Compiled);

        private static readonly Regex FileReferenceLineRegex = new Regex(
            @"^\t\t([0-9A-F]{24}) /\* .* \*/ = \{isa = PBXFileReference;.*\};$",
            RegexOptions.Compiled);

        private static readonly Regex BuildFileLineRegex = new Regex(
            @"^\t\t([0-9A-F]{24}) /\* .* in Sources \*/ = \{isa = PBXBuildFile; fileRef = ([0-9A-F]{24}) /\*",
            RegexOptions.Compiled);

        internal static void CleanXcodeProject(string xcodeProjectPath)
        {
            var pbxProjectPath = Path.Combine(xcodeProjectPath, "Unity-iPhone.xcodeproj", "project.pbxproj");
            if (!File.Exists(pbxProjectPath))
            {
                return;
            }

            var original = File.ReadAllText(pbxProjectPath);
            var cleaned = RemoveLegacyReferences(original);
            if (original == cleaned)
            {
                return;
            }

            File.WriteAllText(pbxProjectPath, cleaned);
            Debug.Log("[MSP iOS] Removed legacy ai.themsp.unity plugin references from Xcode project.");
        }

        private static string RemoveLegacyReferences(string content)
        {
            var legacyFileRefIds = new HashSet<string>();
            foreach (var rawLine in content.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                var match = FileReferenceLineRegex.Match(line);
                if (!match.Success || !LegacyPathRegex.IsMatch(line))
                {
                    continue;
                }

                legacyFileRefIds.Add(match.Groups[1].Value);
            }

            if (legacyFileRefIds.Count == 0)
            {
                return content;
            }

            var legacyBuildFileIds = new HashSet<string>();
            var lines = new List<string>();
            foreach (var rawLine in content.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                var buildMatch = BuildFileLineRegex.Match(line);
                if (buildMatch.Success && legacyFileRefIds.Contains(buildMatch.Groups[2].Value))
                {
                    legacyBuildFileIds.Add(buildMatch.Groups[1].Value);
                    continue;
                }

                var fileRefMatch = FileReferenceLineRegex.Match(line);
                if (fileRefMatch.Success && legacyFileRefIds.Contains(fileRefMatch.Groups[1].Value))
                {
                    continue;
                }

                lines.Add(line);
            }

            var result = string.Join("\n", lines);
            foreach (var id in legacyFileRefIds)
            {
                result = Regex.Replace(result, $@"\t\t\t\t{id} /\* .* \*/,?\r?\n", string.Empty);
            }

            foreach (var id in legacyBuildFileIds)
            {
                result = Regex.Replace(result, $@"\t\t\t\t{id} /\* .* in Sources \*/,?\r?\n", string.Empty);
            }

            result = Regex.Replace(
                result,
                @"\t\t[0-9A-F]{24} /\* ai\.themsp\.unity \*/ = \{\r?\n\t\t\tisa = PBXGroup;\r?\n\t\t\tchildren = \(\r?\n(?:\t\t\t\t[0-9A-F]{24} /\* .* \*/,?\r?\n)*\t\t\t\);\r?\n\t\t\tpath = ai\.themsp\.unity;\r?\n\t\t\tsourceTree = ""<group>"";\r?\n\t\t\};\r?\n",
                string.Empty);

            result = Regex.Replace(
                result,
                @"\t\t\t\t[0-9A-F]{24} /\* ai\.themsp\.unity \*/,?\r?\n",
                string.Empty);

            return result;
        }
    }
}
