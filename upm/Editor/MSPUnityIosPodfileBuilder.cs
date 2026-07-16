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

  installer.aggregate_targets.each do |aggregate_target|
    aggregate_target.user_project.native_targets.each do |native_target|
      next unless ['UnityFramework', 'Unity-iPhone'].include?(native_target.name)
      native_target.build_configurations.each do |config|
        runpaths = config.build_settings['LD_RUNPATH_SEARCH_PATHS'] || ['$(inherited)']
        runpaths = runpaths.to_s.split(/\s+/) unless runpaths.is_a?(Array)
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

  unity_aggregate = installer.aggregate_targets.find do |target|
    target.target_definition.name == 'UnityFramework'
  end
  raise '[MSP iOS] CocoaPods UnityFramework aggregate target was not found' if unity_aggregate.nil?

  # CocoaPods computes these collections from podspec linkage metadata.
  # EmbedFrameworksScript filters XCFrameworks by dynamic build type, excluding
  # static archives such as GoogleMobileAds, MSPSnapKit and SwiftProtobuf.
  runtime_frameworks = unity_aggregate.framework_paths_by_config
  runtime_xcframeworks = unity_aggregate.xcframeworks_by_config
  support_dir = File.join(installer.sandbox.root, 'Target Support Files', 'Pods-UnityFramework')
  FileUtils.mkdir_p(support_dir)
  runtime_script_path = Pathname.new(File.join(support_dir, 'MSP-UnityFramework-runtime-frameworks.sh'))
  Pod::Generator::EmbedFrameworksScript
    .new(runtime_frameworks, runtime_xcframeworks)
    .save_as(runtime_script_path)
  runtime_framework_names = runtime_frameworks.values.flatten.map do |framework|
    File.basename(framework.source_path.to_s, '.framework')
  end
  runtime_framework_names.concat(
    runtime_xcframeworks.values.flatten
      .select {{ |framework| framework.build_type.dynamic_framework? }}
      .map(&:name)
  )
  runtime_framework_names = runtime_framework_names.uniq.sort
  runtime_manifest_path = File.join(support_dir, 'MSP-UnityFramework-runtime-frameworks.txt')
  File.write(runtime_manifest_path, runtime_framework_names.join(""\n"") + ""\n"")

  user_project = unity_aggregate.user_project
  unity_framework_target = user_project.native_targets.find {{ |target| target.name == 'UnityFramework' }}
  unity_app_target = user_project.native_targets.find {{ |target| target.name == 'Unity-iPhone' }}
  raise '[MSP iOS] UnityFramework target was not found' if unity_framework_target.nil?
  raise '[MSP iOS] Unity-iPhone target was not found' if unity_app_target.nil?

  unless unity_app_target.dependencies.any? {{ |dependency| dependency.target == unity_framework_target }}
    unity_app_target.add_dependency(unity_framework_target)
  end

  obsolete_phase_names = [
    '[MSP] Embed Runtime Frameworks',
    '[MSP] Strip Invalid Frameworks',
    '[MSP] Validate Runtime Frameworks'
  ]
  unity_app_target.shell_script_build_phases
    .select {{ |phase| obsolete_phase_names.include?(phase.name) }}
    .each {{ |phase| unity_app_target.build_phases.delete(phase) }}

  embed_phase = user_project.new(Xcodeproj::Project::Object::PBXShellScriptBuildPhase)
  embed_phase.name = '[MSP] Embed Runtime Frameworks'
  embed_phase.shell_path = '/bin/bash'
  embed_phase.always_out_of_date = '1'
  embed_phase.shell_script = <<~SCRIPT
    set -euo pipefail

    RUNTIME_SCRIPT=""${{PODS_ROOT}}/Target Support Files/Pods-UnityFramework/MSP-UnityFramework-runtime-frameworks.sh""
    RUNTIME_MANIFEST=""${{PODS_ROOT}}/Target Support Files/Pods-UnityFramework/MSP-UnityFramework-runtime-frameworks.txt""
    if [ ! -x ""$RUNTIME_SCRIPT"" ]; then
      echo ""error: [MSP iOS] Missing generated runtime framework script: $RUNTIME_SCRIPT""
      exit 1
    fi
    if [ ! -f ""$RUNTIME_MANIFEST"" ]; then
      echo ""error: [MSP iOS] Missing generated runtime framework manifest: $RUNTIME_MANIFEST""
      exit 1
    fi

    echo ""[MSP iOS] Embedding CocoaPods runtime frameworks into $TARGET_BUILD_DIR/$FRAMEWORKS_FOLDER_PATH""
    /bin/bash ""$RUNTIME_SCRIPT""

    APP_FRAMEWORKS=""$TARGET_BUILD_DIR/$FRAMEWORKS_FOLDER_PATH""
    while IFS= read -r name; do
      [ -n ""$name"" ] || continue
      framework=""$APP_FRAMEWORKS/$name.framework""
      if [ ! -d ""$framework"" ]; then
        echo ""error: [MSP iOS] Expected runtime framework was not embedded: $name.framework""
        exit 1
      fi
      binary=""$framework/$name""
      if [ ! -f ""$binary"" ]; then
        echo ""error: [MSP iOS] Embedded runtime framework has no binary: $framework""
        exit 1
      fi
      if ! file ""$binary"" | grep -q 'dynamically linked shared library'; then
        echo ""error: [MSP iOS] Refusing to embed non-dynamic framework: $framework""
        exit 1
      fi
    done < ""$RUNTIME_MANIFEST""
  SCRIPT
  unity_app_target.build_phases << embed_phase

  validate_phase = user_project.new(Xcodeproj::Project::Object::PBXShellScriptBuildPhase)
  validate_phase.name = '[MSP] Validate Runtime Frameworks'
  validate_phase.shell_path = '/bin/bash'
  validate_phase.always_out_of_date = '1'
  validate_phase.shell_script = <<~SCRIPT
    set -euo pipefail

    APP_FRAMEWORKS=""$TARGET_BUILD_DIR/$FRAMEWORKS_FOLDER_PATH""
    UNITY_FRAMEWORK_BINARY=""$TARGET_BUILD_DIR/UnityFramework.framework/UnityFramework""
    if [ ! -f ""$UNITY_FRAMEWORK_BINARY"" ]; then
      echo ""error: [MSP iOS] UnityFramework binary was not built before runtime validation""
      exit 1
    fi

    validate_rpaths() {{
      local binary=""$1""
      local dependencies
      local self_install_name=""@rpath/$(basename ""$(dirname ""$binary"")"")/$(basename ""$binary"")""
      dependencies=""$(otool -L ""$binary"" | awk 'index($1, ""@rpath/"") == 1 && index($1, "".framework/"") > 0 {{ print $1 }}')""
      while IFS= read -r dependency; do
        [ -n ""$dependency"" ] || continue
        [ ""$dependency"" != ""$self_install_name"" ] || continue
        relative_path=""${{dependency\#@rpath/}}""
        if [ ! -e ""$APP_FRAMEWORKS/$relative_path"" ]; then
          echo ""error: [MSP iOS] Missing runtime dependency for $(basename ""$binary""): $dependency""
          return 1
        fi
      done <<< ""$dependencies""
    }}

    validate_rpaths ""$UNITY_FRAMEWORK_BINARY""
    for framework in ""$APP_FRAMEWORKS""/*.framework; do
      [ -d ""$framework"" ] || continue
      name=""$(basename ""$framework"" .framework)""
      binary=""$framework/$name""
      [ -f ""$binary"" ] || continue
      file ""$binary"" | grep -q 'dynamically linked shared library' || continue
      validate_rpaths ""$binary""
      if [ ""${{CODE_SIGNING_ALLOWED:-NO}}"" != ""NO"" ] && [ -n ""${{EXPANDED_CODE_SIGN_IDENTITY:-}}"" ]; then
        /usr/bin/codesign --verify --strict ""$framework""
      fi
    done

    echo ""[MSP iOS] Runtime framework validation passed""
  SCRIPT
  unity_app_target.build_phases << validate_phase

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
            if (string.IsNullOrWhiteSpace(sdkPath) || !System.IO.Directory.Exists(sdkPath))
            {
                UnityEngine.Debug.LogWarning(
                    "[MSP iOS] MSP_UNITY_USE_LOCAL_IOS_SDK=1 requires MSP_IOS_SDK_PATH to point at a valid local iOS SDK checkout.");
                return false;
            }

            escapedSdkPath = PathToEscapedPodPath(sdkPath);
            return true;
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
                    foreach (var thirdParty in ResolveLocalThirdPartyPods())
                    {
                        yield return thirdParty;
                    }

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
