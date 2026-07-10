using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSP.Unity.Adapter;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace MSP.Unity.Editor
{
    internal static class MSPUnityIosAdapterBootstrapEnsurer
    {
        private static readonly string[] KnownAdapterPackageNames =
        {
            "ai.themsp.unity.adapter.nova",
            "ai.themsp.unity.adapter.google",
            "ai.themsp.unity.adapter.facebook",
            "ai.themsp.unity.adapter.unity",
            "ai.themsp.unity.adapter.inmobi",
            "ai.themsp.unity.adapter.mobilefuse",
            "ai.themsp.unity.adapter.mintegral",
            "ai.themsp.unity.adapter.pubmatic",
            "ai.themsp.unity.adapter.moloco",
            "ai.themsp.unity.adapter.amazon",
            "ai.themsp.unity.adapter.liftoff",
            "ai.themsp.unity.adapter.applovin"
        };

        internal static void EnsureBootstrapSources(string xcodeProjectPath)
        {
            var bootstrapFiles = FindExportedBootstrapFiles(xcodeProjectPath);
            CopyMissingBootstrapFiles(xcodeProjectPath, bootstrapFiles);

            if (bootstrapFiles.Count == 0)
            {
                var registeredAdapters = MSPUnityAdapterRegistry.GetAll();
                if (registeredAdapters.Count > 0)
                {
                    var adapterIds = string.Join(", ", registeredAdapters.Select(adapter => adapter.AdapterId));
                    UnityEngine.Debug.LogWarning(
                        $"[MSP iOS] Adapters are installed ({adapterIds}) but no MSPUnity*Bootstrap.swift files were exported. " +
                        "Reimport the adapter packages and rebuild the iOS player.");
                }

                return;
            }

            var pbxProjectPath = PBXProject.GetPBXProjectPath(xcodeProjectPath);
            if (!File.Exists(pbxProjectPath))
            {
                return;
            }

            var pbxProject = new PBXProject();
            pbxProject.ReadFromString(File.ReadAllText(pbxProjectPath));
#if UNITY_2019_3_OR_NEWER
            var frameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
#else
            var frameworkTargetGuid = pbxProject.TargetGuidByName("UnityFramework");
#endif

            var added = 0;
            foreach (var absolutePath in bootstrapFiles)
            {
                var projectRelativePath = ToProjectRelativePath(xcodeProjectPath, absolutePath);
                if (string.IsNullOrEmpty(projectRelativePath))
                {
                    continue;
                }

                var fileGuid = pbxProject.FindFileGuidByProjectPath(projectRelativePath);
                if (string.IsNullOrEmpty(fileGuid))
                {
                    fileGuid = pbxProject.AddFile(absolutePath, projectRelativePath);
                }

                pbxProject.AddFileToBuild(frameworkTargetGuid, fileGuid);
                added++;
                UnityEngine.Debug.Log($"[MSP iOS] Ensured adapter bootstrap source is compiled: {projectRelativePath}");
            }

            if (added > 0)
            {
                File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());
            }
        }

        private static List<string> FindExportedBootstrapFiles(string xcodeProjectPath)
        {
            var librariesRoot = Path.Combine(xcodeProjectPath, "Libraries");
            if (!Directory.Exists(librariesRoot))
            {
                return new List<string>();
            }

            return Directory
                .GetFiles(librariesRoot, "MSPUnity*Bootstrap.swift", SearchOption.AllDirectories)
                .Distinct()
                .ToList();
        }

        private static void CopyMissingBootstrapFiles(string xcodeProjectPath, List<string> bootstrapFiles)
        {
            foreach (var packageName in KnownAdapterPackageNames)
            {
                var packageInfo = GetRegisteredPackage(packageName);
                if (packageInfo == null)
                {
                    continue;
                }

                var sourceRoot = Path.Combine(packageInfo.resolvedPath, "Plugins", "iOS");
                if (!Directory.Exists(sourceRoot))
                {
                    continue;
                }

                foreach (var sourceFile in Directory.GetFiles(sourceRoot, "MSPUnity*Bootstrap.swift"))
                {
                    var fileName = Path.GetFileName(sourceFile);
                    if (bootstrapFiles.Any(path => string.Equals(Path.GetFileName(path), fileName, System.StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    var destinationDirectory = Path.Combine(
                        xcodeProjectPath,
                        "Libraries",
                        packageName,
                        "Plugins",
                        "iOS");
                    Directory.CreateDirectory(destinationDirectory);
                    var destinationFile = Path.Combine(destinationDirectory, fileName);
                    File.Copy(sourceFile, destinationFile, true);
                    bootstrapFiles.Add(destinationFile);
                    UnityEngine.Debug.Log($"[MSP iOS] Copied missing adapter bootstrap source: {destinationFile}");
                }
            }
        }

        private static UnityEditor.PackageManager.PackageInfo GetRegisteredPackage(string packageName)
        {
            foreach (var package in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                if (string.Equals(package.name, packageName, System.StringComparison.Ordinal))
                {
                    return package;
                }
            }

            return null;
        }

        private static string ToProjectRelativePath(string xcodeProjectPath, string absolutePath)
        {
            var normalizedRoot = Path.GetFullPath(xcodeProjectPath).TrimEnd(Path.DirectorySeparatorChar);
            var normalizedFile = Path.GetFullPath(absolutePath);
            if (!normalizedFile.StartsWith(normalizedRoot + Path.DirectorySeparatorChar))
            {
                return string.Empty;
            }

            return normalizedFile
                .Substring(normalizedRoot.Length + 1)
                .Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
