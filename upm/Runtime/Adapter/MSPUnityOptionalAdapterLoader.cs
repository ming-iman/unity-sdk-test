using System;
using System.Reflection;

namespace MSP.Unity.Adapter
{
    internal static class MSPUnityOptionalAdapterLoader
    {
        private static readonly string[] KnownRegistrationTypes =
        {
            "MSP.Unity.Adapter.Nova.NovaAdapterRegistration, MSP.Unity.Adapter.Nova"
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
