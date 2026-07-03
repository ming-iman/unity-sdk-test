using System.Collections.Generic;
using System.Linq;

namespace MSP.Unity.Adapter
{
    public static class MSPUnityAdapterRegistry
    {
        private static readonly List<MSPUnityAdapterDefinition> Adapters = new List<MSPUnityAdapterDefinition>();
        private static bool discovered;

        public static IReadOnlyList<MSPUnityIosPod> CoreIosPodsSource { get; } = new[]
        {
            new MSPUnityIosPod("MSPCore", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.IosPodVersion),
            new MSPUnityIosPod("SwiftProtobuf", MSPUnityIosPodSource.Version, MSPUnityNativeVersions.SwiftProtobufPodVersion)
        };

        public static IReadOnlyList<string> CoreAndroidMavenSpecs { get; } = new[]
        {
            $"ai.themsp:msp-core:{MSPUnityNativeVersions.AndroidMavenVersion}",
            $"ai.themsp:prebid-adapter:{MSPUnityNativeVersions.AndroidMavenVersion}"
        };

        public static void Register(MSPUnityAdapterDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.AdapterId))
            {
                return;
            }

            Adapters.RemoveAll(adapter => adapter.AdapterId == definition.AdapterId);
            Adapters.Add(definition);
        }

        public static IReadOnlyList<MSPUnityAdapterDefinition> GetAll()
        {
            EnsureDiscovered();
            return Adapters.ToList();
        }

        public static IEnumerable<MSPUnityIosPod> GetIosPods()
        {
            EnsureDiscovered();
            foreach (var pod in CoreIosPodsSource)
            {
                yield return pod;
            }

            foreach (var adapter in Adapters)
            {
                foreach (var pod in adapter.IosPods)
                {
                    yield return pod;
                }
            }
        }

        public static bool RequiresGoogleAdsAppId()
        {
            EnsureDiscovered();
            return Adapters.Any(adapter => adapter.RequiresGoogleAdsAppId);
        }

        public static void EnsureDiscovered()
        {
            if (discovered)
            {
                MSPUnityOptionalAdapterLoader.EnsureRegistered();
                return;
            }

            discovered = true;
            MSPUnityAdapterDiscovery.DiscoverAndRegister();
            MSPUnityOptionalAdapterLoader.EnsureRegistered();
        }

        internal static void ResetForTests()
        {
            Adapters.Clear();
            discovered = false;
        }
    }
}
