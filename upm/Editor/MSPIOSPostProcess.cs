using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace MSP.Unity.Editor
{
    internal static class MSPIOSPostProcess
    {
        private const string EnvMspIosSdkPath = "MSP_IOS_SDK_PATH";
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
            UpdateGoogleAdsAppId(pathToBuiltProject);

            var sdkPath = ResolveMspIosSdkPath();
            if (string.IsNullOrEmpty(sdkPath) || !Directory.Exists(sdkPath))
            {
                UnityEngine.Debug.LogWarning(
                    $"[MSP iOS] Skip Podfile generation. MSP iOS SDK path not found. " +
                    $"Set {EnvMspIosSdkPath} to your msp-ios-sdk directory.");
                return;
            }

            WritePodfile(pathToBuiltProject, sdkPath);

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
                appId = ResolveGoogleAdsAppIdFromMspDemo();
            }
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

        private static string ResolveGoogleAdsAppIdFromMspDemo()
        {
            var sdkPath = ResolveMspIosSdkPath();
            if (string.IsNullOrEmpty(sdkPath) || !Directory.Exists(sdkPath))
            {
                return string.Empty;
            }

            var plistPath = Path.Combine(sdkPath, "Examples", "MSPDemoApp", "MSPDemoApp", "Info.plist");
            if (!File.Exists(plistPath))
            {
                return string.Empty;
            }

            try
            {
                var plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));
                return plist.root["GADApplicationIdentifier"]?.AsString() ?? string.Empty;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[MSP iOS] Failed to read GADApplicationIdentifier from MSPDemoApp Info.plist: {e.Message}");
                return string.Empty;
            }
        }

        private static string ResolveMspIosSdkPath()
        {
            var fromEnv = Environment.GetEnvironmentVariable(EnvMspIosSdkPath);
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return Path.GetFullPath(fromEnv.Trim());
            }

            // demo/Assets -> demo -> msp-unity-sdk -> NewsBreak/msp-ios-sdk
            var assetsPath = Application.dataPath;
            var demoRoot = Directory.GetParent(assetsPath)?.FullName;
            var repoRoot = Directory.GetParent(demoRoot ?? string.Empty)?.FullName;
            var newsBreakRoot = Directory.GetParent(repoRoot ?? string.Empty)?.FullName;
            if (string.IsNullOrEmpty(newsBreakRoot))
            {
                return string.Empty;
            }

            return Path.Combine(newsBreakRoot, "msp-ios-sdk");
        }

        private static void WritePodfile(string xcodeProjectPath, string mspIosSdkPath)
        {
            var escapedSdkPath = mspIosSdkPath.Replace("\\", "/").Replace("'", "\\'");
            var podfileContent =
$@"platform :ios, '15.0'
use_frameworks! :linkage => :static
use_modular_headers!
inhibit_all_warnings!

target 'UnityFramework' do
  pod 'MSPiOSCore', :path => '{escapedSdkPath}'
  pod 'NovaCore', :path => '{escapedSdkPath}'
  pod 'MSPCore', :path => '{escapedSdkPath}'
  pod 'MSPPrebidAdapter', :path => '{escapedSdkPath}'
  pod 'MSPGoogleAdapter', :path => '{escapedSdkPath}'
  pod 'MSPNovaAdapter', :path => '{escapedSdkPath}'
  pod 'MSPSharedLibraries', :path => '{escapedSdkPath}'
  pod 'MSPGoogleAdsTypes', :path => '{escapedSdkPath}'
  pod 'MSPKingfisher', :path => '{escapedSdkPath}/ThirdParty/MSPKingfisher'
  pod 'MSPSnapKit', :path => '{escapedSdkPath}/ThirdParty/MSPSnapKit'
  pod 'SwiftProtobuf', '~> 1.28.2'
  pod 'Google-Mobile-Ads-SDK', '~> 12.0'
end

target 'Unity-iPhone' do
  inherit! :search_paths
end

post_install do |installer|
  remove_privacy = Proc.new do |resources_phase|
    next unless resources_phase
    resources_phase.files.to_a.each do |build_file|
      ref = build_file.file_ref
      next unless ref
      path = ref.path.to_s
      next unless path.end_with?('PrivacyInfo.xcprivacy')
      resources_phase.remove_build_file(build_file)
    end
  end

  # 1) Pods project aggregate target
  installer.pods_project.targets.each do |target|
    next unless target.name == 'Pods-UnityFramework'
    remove_privacy.call(target.resources_build_phase)
  end

  # 1.5) Ensure Swift pod modules are installable/importable by downstream pods.
  # This avoids cases where Swift symbols are invisible even though the pod builds.
  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      config.build_settings['SWIFT_INSTALL_MODULE_FOR_DEPLOYMENT'] = 'YES'
      config.build_settings['DEFINES_MODULE'] = 'YES'
    end
  end

  # 2) User project target (UnityFramework) where [CP] scripts attach resources
  installer.aggregate_targets.each do |aggregate_target|
    aggregate_target.user_project.native_targets.each do |native_target|
      next unless native_target.name == 'UnityFramework'
      remove_privacy.call(native_target.resources_build_phase)
    end
  end

  # 2.5) Ensure required dynamic frameworks are embedded into app bundle.
  # Pods static linkage does not always generate an embed script for these frameworks.
  installer.aggregate_targets.each do |aggregate_target|
    aggregate_target.user_project.native_targets.each do |native_target|
      next unless native_target.name == 'Unity-iPhone'
      phase_name = '[MSP] Embed Runtime Frameworks'
      phase = native_target.shell_script_build_phases.find do |p|
        p.name == phase_name
      end
      if phase.nil?
        phase = aggregate_target.user_project.new(Xcodeproj::Project::Object::PBXShellScriptBuildPhase)
        phase.name = phase_name
        native_target.build_phases << phase
      end
      phase.shell_script = <<~SCRIPT
        set -e
        DST=""$TARGET_BUILD_DIR/$FRAMEWORKS_FOLDER_PATH""
        mkdir -p ""$DST""

        embed_framework() {{
          local src=""$1""
          local name=""$2""
          if [ -d ""$src"" ]; then
            rsync -a --delete ""$src"" ""$DST""
            if [ -n ""$EXPANDED_CODE_SIGN_IDENTITY"" ]; then
              /usr/bin/codesign --force --sign ""$EXPANDED_CODE_SIGN_IDENTITY"" --preserve-metadata=identifier,entitlements ""$DST/$name""
            fi
            echo ""[MSP iOS] Embedded $name""
          else
            echo ""[MSP iOS] Skip embedding $name (not found at $src)""
          fi
        }}

        embed_framework ""$PODS_XCFRAMEWORKS_BUILD_DIR/NovaCore/OMSDK_Newsbreak1.framework"" ""OMSDK_Newsbreak1.framework""
        embed_framework ""$PODS_XCFRAMEWORKS_BUILD_DIR/MSPSharedLibraries/PrebidMobile.framework"" ""PrebidMobile.framework""
      SCRIPT
    end
  end

  # 3) Hard fallback: dedupe duplicate PrivacyInfo.xcprivacy lines
  # in CocoaPods generated resources script. Also remove all privacy entries
  # so Pods do not produce UnityFramework.framework/PrivacyInfo.xcprivacy.
  resources_script = File.join(
    installer.sandbox.root,
    'Target Support Files',
    'Pods-UnityFramework',
    'Pods-UnityFramework-resources.sh'
  )
  if File.exist?(resources_script)
    lines = File.readlines(resources_script)
    filtered = lines.reject do |line|
      line.include?('install_resource') && line.include?('PrivacyInfo.xcprivacy')
    end
    File.write(resources_script, filtered.join)
  end

  # 4) Remove PrivacyInfo.xcprivacy input/output paths from [CP] Copy Pods Resources
  # in user project to avoid duplicate ""Multiple commands produce"" with Unity's own privacy file.
  installer.aggregate_targets.each do |aggregate_target|
    aggregate_target.user_project.native_targets.each do |native_target|
      next unless native_target.name == 'UnityFramework'
      native_target.shell_script_build_phases.each do |phase|
        next unless phase.name == '[CP] Copy Pods Resources'
        if phase.input_paths
          phase.input_paths = phase.input_paths.reject {{ |p| p.to_s.include?('PrivacyInfo.xcprivacy') }}
        end
        if phase.output_paths
          phase.output_paths = phase.output_paths.reject {{ |p| p.to_s.include?('PrivacyInfo.xcprivacy') }}
        end
      end
    end
  end
end
";

            var podfilePath = Path.Combine(xcodeProjectPath, "Podfile");
            File.WriteAllText(podfilePath, podfileContent);
            UnityEngine.Debug.Log($"[MSP iOS] Podfile generated: {podfilePath}");
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
