using System;
using System.Linq;
using System.Reflection;

namespace DofusBatteriesIncluded.Plugins.Core.Extensions;

static class AppDomainExtensions
{
    public static Assembly LoadAssemblyIfNotLoaded(this AppDomain appDomain, string assemblyName, string assemblyPath)
    {
        Assembly assembly = appDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);

        if (assembly == null)
        {
            // assembly not loaded
            assembly = Assembly.LoadFrom(assemblyPath);
        }

        return assembly;
    }
}
