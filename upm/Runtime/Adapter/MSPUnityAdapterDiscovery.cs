using System;
using System.Linq;
using System.Reflection;

namespace MSP.Unity.Adapter
{
    internal static class MSPUnityAdapterDiscovery
    {
        public static void DiscoverAndRegister()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in SafeGetTypes(assembly))
                {
                    if (type == null || !typeof(IMSPUnityAdapterContributor).IsAssignableFrom(type) || type.IsAbstract)
                    {
                        continue;
                    }

                    try
                    {
                        if (Activator.CreateInstance(type) is IMSPUnityAdapterContributor contributor)
                        {
                            MSPUnityAdapterRegistry.Register(contributor.CreateDefinition());
                        }
                    }
                    catch (Exception)
                    {
                        // Optional adapter assemblies may not be present in every project.
                    }
                }
            }
        }

        private static Type[] SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types ?? Array.Empty<Type>();
            }
        }
    }
}
