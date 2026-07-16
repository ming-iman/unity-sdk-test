using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MSP.Unity.Adapter;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace MSP.Unity.Editor
{
    internal static class MSPIOSPostProcess
    {
        private const string EnvMspIosSdkPath = "MSP_IOS_SDK_PATH";
        private const string EnvUseLocalIosSdk = "MSP_UNITY_USE_LOCAL_IOS_SDK";
        private const string EnvSkipPodInstall = "MSP_UNITY_SKIP_POD_INSTALL";
        private const string EnvMspIosBundleId = "MSP_UNITY_IOS_BUNDLE_ID";
        private const string DefaultMspIosBundleId = "MSPDemoApp.MSPDemoApp";
        private const string EnvGoogleAdsAppId = "MSP_UNITY_GAD_APP_ID";
        private const string DefaultGoogleAdsAppId = "ca-app-pub-3940256099942544~1458002511";

        [PostProcessBuild(999)]
        private static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            UpdateBundleIdentifier(pathToBuiltProject);
            MSPUnityLegacyPackageCleaner.CleanXcodeProject(pathToBuiltProject);
            MSPUnityAdapterRegistry.EnsureDiscovered();
            MSPUnityIosAdapterBootstrapEnsurer.EnsureBootstrapSources(pathToBuiltProject);
            if (MSPUnityAdapterRegistry.RequiresGoogleAdsAppId())
            {
                UpdateGoogleAdsAppId(pathToBuiltProject);
            }

            WritePodfile(pathToBuiltProject);

            var skipPodInstall = string.Equals(
                Environment.GetEnvironmentVariable(EnvSkipPodInstall),
                "1",
                StringComparison.Ordinal);
            if (skipPodInstall)
            {
                UnityEngine.Debug.Log("[MSP iOS] Skipped pod install due to MSP_UNITY_SKIP_POD_INSTALL=1.");
                return;
            }

            RunPodInstall(pathToBuiltProject);
            MSPUnityLegacyPackageCleaner.CleanXcodeProject(pathToBuiltProject);
            MSPUnityIosAdapterBootstrapEnsurer.EnsureBootstrapSources(pathToBuiltProject);
        }

        [PostProcessBuild(2000)]
        private static void OnPostProcessBuildLateCleanup(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            MSPUnityLegacyPackageCleaner.CleanXcodeProject(pathToBuiltProject);
        }

        private static void UpdateBundleIdentifier(string xcodeProjectPath)
        {
            var bundleId = Environment.GetEnvironmentVariable(EnvMspIosBundleId);
            if (string.IsNullOrWhiteSpace(bundleId))
            {
                bundleId = DefaultMspIosBundleId;
            }

            var pbxProjectPath = PBXProject.GetPBXProjectPath(xcodeProjectPath);
            if (!File.Exists(pbxProjectPath))
            {
                UnityEngine.Debug.LogWarning($"[MSP iOS] Skip bundle id update. PBX project not found: {pbxProjectPath}");
                return;
            }

            var pbxProject = new PBXProject();
            pbxProject.ReadFromString(File.ReadAllText(pbxProjectPath));
#if UNITY_2019_3_OR_NEWER
            var mainTargetGuid = pbxProject.GetUnityMainTargetGuid();
#else
            var mainTargetGuid = pbxProject.TargetGuidByName("Unity-iPhone");
#endif
            pbxProject.SetBuildProperty(mainTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", bundleId);
            File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());
            UnityEngine.Debug.Log($"[MSP iOS] Set Unity-iPhone bundle id: {bundleId}");
        }

        private static void UpdateGoogleAdsAppId(string xcodeProjectPath)
        {
            var appId = Environment.GetEnvironmentVariable(EnvGoogleAdsAppId);
            if (string.IsNullOrWhiteSpace(appId))
            {
                appId = DefaultGoogleAdsAppId;
            }

            var plistPath = Path.Combine(xcodeProjectPath, "Info.plist");
            if (!File.Exists(plistPath))
            {
                UnityEngine.Debug.LogWarning($"[MSP iOS] Skip GAD app id update. Info.plist not found: {plistPath}");
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            plist.root.SetString("GADApplicationIdentifier", appId);
            File.WriteAllText(plistPath, plist.WriteToString());
            UnityEngine.Debug.Log($"[MSP iOS] Set GADApplicationIdentifier: {appId}");
        }

        private static string ResolveMspIosSdkPath()
        {
            var fromEnv = Environment.GetEnvironmentVariable(EnvMspIosSdkPath);
            if (string.IsNullOrWhiteSpace(fromEnv))
            {
                return string.Empty;
            }

            return Path.GetFullPath(fromEnv.Trim());
        }

        private static void WritePodfile(string xcodeProjectPath)
        {
            var podfileContent = MSPUnityIosPodfileBuilder.Build();
            var adapters = MSPUnityAdapterRegistry.GetAll();
            var adapterSummary = adapters.Count == 0
                ? "core-only"
                : string.Join(", ", adapters.Select(adapter => adapter.AdapterId));
            var sourceMode = UsesLocalIosSdkPods()
                ? "local-path"
                : $"cocoapods-trunk ({MSPUnityNativeVersions.IosPodVersion})";

            var podfilePath = Path.Combine(xcodeProjectPath, "Podfile");
            File.WriteAllText(podfilePath, podfileContent);
            UnityEngine.Debug.Log($"[MSP iOS] Podfile generated ({adapterSummary}, {sourceMode}): {podfilePath}");
        }

        private static bool UsesLocalIosSdkPods()
        {
            if (!string.Equals(Environment.GetEnvironmentVariable(EnvUseLocalIosSdk), "1", StringComparison.Ordinal))
            {
                return false;
            }

            return !string.IsNullOrEmpty(ResolveMspIosSdkPath());
        }

        private static void RunPodInstall(string xcodeProjectPath)
        {
            var sdkPath = ResolveMspIosSdkPath();
            var sdkGemfile = string.IsNullOrEmpty(sdkPath)
                ? string.Empty
                : Path.Combine(sdkPath, "Gemfile");
            var bundlePath = FindExecutable("bundle");
            var podPath = FindExecutable("pod");

            if (string.IsNullOrEmpty(podPath))
            {
                UnityEngine.Debug.LogError(
                    "[MSP iOS] Cannot find `pod` executable. " +
                    "Please ensure CocoaPods is installed and available in PATH/rbenv/homebrew.");
                return;
            }

            if (File.Exists(sdkGemfile) && !string.IsNullOrEmpty(bundlePath))
            {
                UnityEngine.Debug.Log($"[MSP iOS] Running bundle exec pod install (Gemfile: {sdkGemfile})");
                var bundleResult = RunProcess(
                    xcodeProjectPath,
                    bundlePath,
                    $"exec {podPath} install",
                    sdkGemfile);
                if (bundleResult.exitCode == 0)
                {
                    if (!string.IsNullOrEmpty(bundleResult.stdout))
                    {
                        UnityEngine.Debug.Log($"[MSP iOS][pod] {bundleResult.stdout}");
                    }
                    UnityEngine.Debug.Log("[MSP iOS] pod install completed via bundle exec.");
                    return;
                }

                UnityEngine.Debug.LogWarning(
                    $"[MSP iOS] bundle exec pod install failed (exit {bundleResult.exitCode}), fallback to plain pod install.\n" +
                    $"stdout:\n{bundleResult.stdout}\n" +
                    $"stderr:\n{bundleResult.stderr}");
            }
            else
            {
                UnityEngine.Debug.Log(
                    File.Exists(sdkGemfile)
                        ? "[MSP iOS] `bundle` not found, fallback to plain pod install."
                        : "[MSP iOS] Gemfile not found, fallback to plain pod install.");
            }

            var podResult = RunProcess(xcodeProjectPath, podPath, "install", null);
            if (!string.IsNullOrEmpty(podResult.stdout))
            {
                UnityEngine.Debug.Log($"[MSP iOS][pod] {podResult.stdout}");
            }

            if (podResult.exitCode != 0)
            {
                UnityEngine.Debug.LogError(
                    $"[MSP iOS] pod install failed (exit {podResult.exitCode}).\n" +
                    $"working_dir={xcodeProjectPath}\n" +
                    $"stdout:\n{podResult.stdout}\n" +
                    $"stderr:\n{podResult.stderr}");
                return;
            }

            UnityEngine.Debug.Log("[MSP iOS] pod install completed.");
        }

        private static (int exitCode, string stdout, string stderr) RunProcess(
            string workingDirectory,
            string executable,
            string arguments,
            string gemfilePath)
        {
            var psi = new ProcessStartInfo
            {
                // Invoke executable directly to avoid loading user shell profiles (.bash_profile/.zshrc),
                // which can break non-interactive builds.
                FileName = executable,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.Environment["LANG"] = "en_US.UTF-8";
            psi.Environment["LC_ALL"] = "en_US.UTF-8";
            if (!string.IsNullOrEmpty(gemfilePath))
            {
                psi.Environment["BUNDLE_GEMFILE"] = gemfilePath;
            }

            using var process = Process.Start(psi);
            if (process == null)
            {
                return (-1, string.Empty, $"Failed to start process: {executable} {arguments}");
            }

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, stdout, stderr);
        }

        private static string FindExecutable(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var candidates = new List<string>();
            var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var segment in pathVar.Split(Path.PathSeparator))
            {
                if (!string.IsNullOrWhiteSpace(segment))
                {
                    candidates.Add(Path.Combine(segment, name));
                }
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(home))
            {
                candidates.Add(Path.Combine(home, ".rbenv", "shims", name));
                candidates.Add(Path.Combine(home, ".asdf", "shims", name));
            }

            candidates.Add(Path.Combine("/opt/homebrew/bin", name));
            candidates.Add(Path.Combine("/usr/local/bin", name));
            candidates.Add(Path.Combine("/usr/bin", name));

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }
    }
}
