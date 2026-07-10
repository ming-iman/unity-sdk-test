using System;
using System.Reflection;

namespace MSP.Unity.Adapter
{
    internal static class MSPUnityOptionalAdapterLoader
    {
        private static readonly string[] KnownRegistrationTypes =
        {
            "MSP.Unity.Adapter.Nova.NovaAdapterRegistration, MSP.Unity.Adapter.Nova",
            "MSP.Unity.Adapter.Google.GoogleAdapterRegistration, MSP.Unity.Adapter.Google",
            "MSP.Unity.Adapter.Facebook.FacebookAdapterRegistration, MSP.Unity.Adapter.Facebook",
            "MSP.Unity.Adapter.UnityAds.UnityAdsAdapterRegistration, MSP.Unity.Adapter.UnityAds",
            "MSP.Unity.Adapter.Inmobi.InmobiAdapterRegistration, MSP.Unity.Adapter.Inmobi",
            "MSP.Unity.Adapter.Mobilefuse.MobilefuseAdapterRegistration, MSP.Unity.Adapter.Mobilefuse",
            "MSP.Unity.Adapter.Mintegral.MintegralAdapterRegistration, MSP.Unity.Adapter.Mintegral",
            "MSP.Unity.Adapter.Pubmatic.PubmaticAdapterRegistration, MSP.Unity.Adapter.Pubmatic",
            "MSP.Unity.Adapter.Moloco.MolocoAdapterRegistration, MSP.Unity.Adapter.Moloco",
            "MSP.Unity.Adapter.Amazon.AmazonAdapterRegistration, MSP.Unity.Adapter.Amazon",
            "MSP.Unity.Adapter.Liftoff.LiftoffAdapterRegistration, MSP.Unity.Adapter.Liftoff",
            "MSP.Unity.Adapter.Applovin.ApplovinAdapterRegistration, MSP.Unity.Adapter.Applovin"
        };

        public static void EnsureRegistered()
        {
            foreach (var typeName in KnownRegistrationTypes)
            {
                TryInvokeRegister(typeName);
            }
        }

        public static bool IsAssemblyLoaded(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(assembly.GetName().Name, assemblyName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void TryInvokeRegister(string assemblyQualifiedTypeName)
        {
            var type = Type.GetType(assemblyQualifiedTypeName, throwOnError: false);
            if (type == null)
            {
                return;
            }

            var register = type.GetMethod(
                "Register",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            register?.Invoke(null, null);
        }
    }
}
