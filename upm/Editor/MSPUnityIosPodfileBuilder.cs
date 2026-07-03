using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSP.Unity.Adapter;

namespace MSP.Unity.Editor
{
    internal static class MSPUnityIosPodfileBuilder
    {
        internal static string Build(string escapedSdkPath)
        {
            MSPUnityAdapterRegistry.EnsureDiscovered();

            var podLines = new List<string>();
            var seenPods = new HashSet<string>();

            foreach (var pod in MSPUnityAdapterRegistry.GetIosPods())
            {
                if (!seenPods.Add(pod.Name))
                {
                    continue;
                }

                podLines.Add(FormatPodLine(pod, escapedSdkPath));
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
                    return $"pod '{pod.Name}', :path => '{escapedSdkPath}'";
            }
        }
    }
}
