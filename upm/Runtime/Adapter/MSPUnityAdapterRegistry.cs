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
            new MSPUnityIosPod("MSPiOSCore", MSPUnityIosPodSource.SdkRoot),
            new MSPUnityIosPod("MSPCore", MSPUnityIosPodSource.SdkRoot),
            new MSPUnityIosPod("MSPPrebidAdapter", MSPUnityIosPodSource.SdkRoot),
            new MSPUnityIosPod("MSPSharedLibraries", MSPUnityIosPodSource.SdkRoot),
            new MSPUnityIosPod("SwiftProtobuf", MSPUnityIosPodSource.Version, "~> 1.28.2")
        };

        public static IReadOnlyList<string> CoreAndroidMavenSpecs { get; } = new[]
        {
            "ai.themsp:msp-core:4.1.0",
            "ai.themsp:prebid-adapter:4.1.0"
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
