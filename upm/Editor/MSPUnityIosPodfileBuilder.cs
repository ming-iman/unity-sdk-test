using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSP.Unity.Adapter;

namespace MSP.Unity.Editor
{
    internal static class MSPUnityIosPodfileBuilder
    {
        private const string EnvMspIosSdkPath = "MSP_IOS_SDK_PATH";
        private const string EnvUseLocalIosSdk = "MSP_UNITY_USE_LOCAL_IOS_SDK";

        private static readonly IReadOnlyDictionary<string, string> LocalThirdPartyPodPaths =
            new Dictionary<string, string>
            {
                { "MSPKingfisher", "ThirdParty/MSPKingfisher" },
                { "MSPSnapKit", "ThirdParty/MSPSnapKit" }
            };

        internal static string Build()
        {
            MSPUnityAdapterRegistry.EnsureDiscovered();

            var useLocalSdkPath = ShouldUseLocalIosSdkPath(out var escapedSdkPath);
            var podLines = new List<string>();
            var seenPods = new HashSet<string>();

            foreach (var pod in MSPUnityAdapterRegistry.GetIosPods())
            {
                foreach (var resolvedPod in ResolvePods(pod, useLocalSdkPath))
                {
                    if (!seenPods.Add(resolvedPod.Name))
                    {
                        continue;
                    }

                    podLines.Add(FormatPodLine(resolvedPod, escapedSdkPath));
                }
            }

            var podBlock = string.Join("\n", podLines.Select(line => $"  {line}"));

            return $@"platform :ios, '15.0'
use_frameworks! :linkage => :static
use_modular_headers!
inhibit_all_warnings!

target 'UnityFramework' do
{podBlock}
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

  installer.pods_project.targets.each do |target|
    next unless target.name == 'Pods-UnityFramework'
    remove_privacy.call(target.resources_build_phase)
  end

  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      config.build_settings['SWIFT_INSTALL_MODULE_FOR_DEPLOYMENT'] = 'YES'
      config.build_settings['DEFINES_MODULE'] = 'YES'
    end
  end

  blocked_embed_frameworks = %w[MSPSnapKit MSPKingfisher MSPNovaAdapter]
  strip_blocked_framework_lines = Proc.new do |path|
    next unless File.exist?(path)
    lines = File.readlines(path)
    filtered = lines.reject do |line|
      blocked_embed_frameworks.any? {{ |name| line.include?(name) }}
    end
    next if filtered == lines
    File.write(path, filtered.join)
  end

  pods_support_dir = File.join(installer.sandbox.root, 'Target Support Files', 'Pods-UnityFramework')
  if Dir.exist?(pods_support_dir)
    Dir.glob(File.join(pods_support_dir, '*')).each do |path|
      next unless File.file?(path)
      next unless path.include?('frameworks')
      strip_blocked_framework_lines.call(path)
    end
  end

  installer.aggregate_targets.each do |aggregate_target|
    aggregate_target.user_project.native_targets.each do |native_target|
      next unless ['UnityFramework', 'Unity-iPhone'].include?(native_target.name)
      native_target.build_configurations.each do |config|
        runpaths = config.build_settings['LD_RUNPATH_SEARCH_PATHS'] || ['$(inherited)']
        runpaths = [runpaths] unless runpaths.is_a?(Array)
        ['@executable_path/Frameworks', '@loader_path/Frameworks'].each do |entry|
          runpaths << entry unless runpaths.include?(entry)
        end
        config.build_settings['LD_RUNPATH_SEARCH_PATHS'] = runpaths
      end
    end
  end

  installer.aggregate_targets.each do |aggregate_target|
    aggregate_target.user_project.native_targets.each do |native_target|
      next unless native_target.name == 'UnityFramework'
      remove_privacy.call(native_target.resources_build_phase)
    end
  end

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
        APP_DST=""$TARGET_BUILD_DIR/$FRAMEWORKS_FOLDER_PATH""
        for blocked in MSPSnapKit MSPKingfisher MSPNovaAdapter; do
          rm -rf ""$APP_DST/${{blocked}}.framework""
        done

        FRAMEWORKS_SCRIPT=""${{PODS_ROOT}}/Target Support Files/Pods-UnityFramework/Pods-UnityFramework-frameworks.sh""
        if [ -f ""$FRAMEWORKS_SCRIPT"" ]; then
          echo ""[MSP iOS] Embedding pod frameworks via CocoaPods script""
          /bin/bash ""$FRAMEWORKS_SCRIPT""
        else
          echo ""[MSP iOS] Warning: Pods-UnityFramework-frameworks.sh not found""
        fi

        embed_dylib_framework() {{
          local src=""$1""
          local name=""$2""
          if [ ! -d ""$src"" ] || [ ! -f ""$src/Info.plist"" ]; then
            return 1
          fi
          local bin=""$src/$name""
          if [ ! -f ""$bin"" ] || file ""$bin"" | grep -q 'ar archive'; then
            return 1
          fi
          local dst=""$APP_DST/${{name}}.framework""
          ditto ""$src"" ""$dst""
          if [ ""${{CODE_SIGNING_REQUIRED}}"" = ""YES"" ] && [ -n ""${{EXPANDED_CODE_SIGN_IDENTITY}}"" ] && [ ""${{EXPANDED_CODE_SIGN_IDENTITY}}"" != ""-"" ]; then
            /usr/bin/codesign --force --sign ""${{EXPANDED_CODE_SIGN_IDENTITY}}"" ${{OTHER_CODE_SIGN_FLAGS}} --preserve-metadata=identifier,entitlements,flags --timestamp=none ""$dst""
            echo ""[MSP iOS] Code signed ${{name}}.framework""
          else
            echo ""[MSP iOS] Warning: skipped codesign for ${{name}}.framework (no signing identity)""
          fi
          echo ""[MSP iOS] Embedded ${{name}}.framework""
          return 0
        }}

        for omsdk_src in \
          ""$PODS_XCFRAMEWORKS_BUILD_DIR/MSPSharedLibraries/OMSDK_Newsbreak1.framework"" \
          ""$PODS_XCFRAMEWORKS_BUILD_DIR/NovaCore/OMSDK_Newsbreak1.framework""; do
          if embed_dylib_framework ""$omsdk_src"" ""OMSDK_Newsbreak1""; then
            break
          fi
        done

        for fw in ""$APP_DST""/*.framework; do
          [ -d ""$fw"" ] || continue
          fw_name=""$(basename ""$fw"" .framework)""
          fw_bin=""$fw/$fw_name""
          [ -f ""$fw_bin"" ] || continue
          if file ""$fw_bin"" | grep -q 'ar archive'; then
            echo ""[MSP iOS] Removing static framework ${{fw_name}} from app bundle""
            rm -rf ""$fw""
          fi
        done
      SCRIPT
      native_target.build_phases.delete(phase)
      native_target.build_phases << phase
    end
  end

  installer.aggregate_targets.each do |aggregate_target|
    aggregate_target.user_project.native_targets.each do |native_target|
      next unless native_target.name == 'Unity-iPhone'
      phase_name = '[MSP] Strip Invalid Frameworks'
      phase = native_target.shell_script_build_phases.find do |p|
        p.name == phase_name
      end
      if phase.nil?
        phase = aggregate_target.user_project.new(Xcodeproj::Project::Object::PBXShellScriptBuildPhase)
        phase.name = phase_name
        native_target.build_phases << phase
      end
      phase.shell_script = <<~SCRIPT
        APP_DST=""$TARGET_BUILD_DIR/$FRAMEWORKS_FOLDER_PATH""
        for blocked in MSPSnapKit MSPKingfisher MSPNovaAdapter; do
          rm -rf ""$APP_DST/${{blocked}}.framework""
        done

        for fw in ""$APP_DST""/*.framework; do
          [ -d ""$fw"" ] || continue
          fw_name=""$(basename ""$fw"" .framework)""
          fw_bin=""$fw/$fw_name""
          [ -f ""$fw_bin"" ] || continue
          if file ""$fw_bin"" | grep -q 'ar archive'; then
            echo ""[MSP iOS] Removing static framework ${{fw_name}} from app bundle""
            rm -rf ""$fw""
          fi
        done
      SCRIPT
      native_target.build_phases.delete(phase)
      native_target.build_phases << phase
    end
  end

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
        }

        private static bool ShouldUseLocalIosSdkPath(out string escapedSdkPath)
        {
            escapedSdkPath = string.Empty;
            if (!string.Equals(
                    Environment.GetEnvironmentVariable(EnvUseLocalIosSdk),
                    "1",
                    StringComparison.Ordinal))
            {
                return false;
            }

            var sdkPath = Environment.GetEnvironmentVariable(EnvMspIosSdkPath);
            if (string.IsNullOrWhiteSpace(sdkPath))
            {
                sdkPath = ResolveDefaultLocalSdkPath();
            }

            if (string.IsNullOrWhiteSpace(sdkPath) || !System.IO.Directory.Exists(sdkPath))
            {
                return false;
            }

            escapedSdkPath = PathToEscapedPodPath(sdkPath);
            return true;
        }

        private static string ResolveDefaultLocalSdkPath()
        {
            var assetsPath = UnityEngine.Application.dataPath;
            var demoRoot = System.IO.Directory.GetParent(assetsPath)?.FullName;
            var repoRoot = System.IO.Directory.GetParent(demoRoot ?? string.Empty)?.FullName;
            var newsBreakRoot = System.IO.Directory.GetParent(repoRoot ?? string.Empty)?.FullName;
            if (string.IsNullOrEmpty(newsBreakRoot))
            {
                return string.Empty;
            }

            var sdkPath = System.IO.Path.Combine(newsBreakRoot, "msp-ios-sdk");
            return System.IO.Directory.Exists(sdkPath) ? sdkPath : string.Empty;
        }

        private static string PathToEscapedPodPath(string sdkPath)
        {
            return sdkPath.Replace("\\", "/").Replace("'", "\\'");
        }

        private static IEnumerable<MSPUnityIosPod> ResolvePods(MSPUnityIosPod pod, bool useLocalSdkPath)
        {
            if (!useLocalSdkPath || pod.Source != MSPUnityIosPodSource.Version)
            {
                yield return pod;
                yield break;
            }

            switch (pod.Name)
            {
                case "MSPCore":
                    yield return new MSPUnityIosPod("MSPiOSCore", MSPUnityIosPodSource.SdkRoot);
                    yield return new MSPUnityIosPod("MSPCore", MSPUnityIosPodSource.SdkRoot);
                    yield return new MSPUnityIosPod("MSPPrebidAdapter", MSPUnityIosPodSource.SdkRoot);
                    yield return new MSPUnityIosPod("MSPSharedLibraries", MSPUnityIosPodSource.SdkRoot);
                    yield return pod;
                    yield break;
                case "MSPNovaAdapter":
                    yield return new MSPUnityIosPod("MSPNovaAdapter", MSPUnityIosPodSource.SdkRoot);
                    yield return new MSPUnityIosPod("NovaCore", MSPUnityIosPodSource.SdkRoot);
                    foreach (var thirdParty in ResolveLocalThirdPartyPods())
                    {
                        yield return thirdParty;
                    }

                    yield break;
                case "MSPGoogleAdapter":
                case "MSPFacebookAdapter":
                case "UnityAdapter":
                case "InmobiAdapter":
                case "MobilefuseAdapter":
                case "MintegralAdapter":
                case "PubmaticAdapter":
                case "MSPMolocoAdapter":
                case "MSPAmazonAdapter":
                case "MSPLiftoffAdapter":
                    yield return new MSPUnityIosPod(pod.Name, MSPUnityIosPodSource.SdkRoot);
                    yield break;
                case "MSPApplovinMaxAdapter":
                    yield return new MSPUnityIosPod("MSPApplovinMaxAdapter", MSPUnityIosPodSource.SdkRoot);
                    foreach (var thirdParty in ResolveLocalThirdPartyPods())
                    {
                        yield return thirdParty;
                    }

                    yield break;
                default:
                    yield return pod;
                    yield break;
            }
        }

        private static IEnumerable<MSPUnityIosPod> ResolveLocalThirdPartyPods()
        {
            if (LocalThirdPartyPodPaths.TryGetValue("MSPKingfisher", out var kingfisherPath))
            {
                yield return new MSPUnityIosPod("MSPKingfisher", MSPUnityIosPodSource.SdkThirdParty, relativePath: kingfisherPath);
            }

            if (LocalThirdPartyPodPaths.TryGetValue("MSPSnapKit", out var snapKitPath))
            {
                yield return new MSPUnityIosPod("MSPSnapKit", MSPUnityIosPodSource.SdkThirdParty, relativePath: snapKitPath);
            }
        }

        private static string FormatPodLine(MSPUnityIosPod pod, string escapedSdkPath)
        {
            switch (pod.Source)
            {
                case MSPUnityIosPodSource.SdkRoot:
                    return $"pod '{pod.Name}', :path => '{escapedSdkPath}'";
                case MSPUnityIosPodSource.SdkThirdParty:
                    var relativePath = string.IsNullOrWhiteSpace(pod.RelativePath)
                        ? pod.Name
                        : pod.RelativePath;
                    return $"pod '{pod.Name}', :path => '{escapedSdkPath}/{relativePath}'";
                case MSPUnityIosPodSource.Version:
                    return $"pod '{pod.Name}', '{pod.Version}'";
                default:
                    return $"pod '{pod.Name}', '{pod.Version}'";
            }
        }
    }
}
