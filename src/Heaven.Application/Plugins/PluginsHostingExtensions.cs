using System.Reflection;
using System.Runtime.Loader;
using Heaven.Abstractions;

namespace DBI.Heaven.Application.Plugins;

static class PluginsHostingExtensions
{
    public static void ConfigurePlugins(this IServiceCollection services, ILogger? logger = null)
    {
        services.Configure<PluginsOptions>(opt => opt.PluginTypes.AddRange(FindPluginsInAssembliesAtPaths(logger, "TestPlugin.dll")));
        services.AddSingleton<PluginInstancesService>();
        services.AddHostedService<PluginsHost>();
    }

    static IEnumerable<Type> FindPluginsInAssembliesAtPaths(ILogger? logger, params string[] paths)
    {
        AssemblyLoadContext loadContext = new("DBI Plugins");

        string thisAssemblyPath = AppContext.BaseDirectory;
        string? thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);

        return paths.SelectMany(
            path =>
            {
                string otherAssemblyPath = Path.Join(thisAssemblyDirectory, path);
                if (!File.Exists(otherAssemblyPath))
                {
                    logger?.LogInformation("Could not find plugin {RelativePath}, looked at {AbsolutePath}.", path, otherAssemblyPath);
                    return [];
                }

                Assembly assembly = loadContext.LoadFromAssemblyPath(otherAssemblyPath);
                return FindPluginsInAssembly(assembly);
            }
        );
    }

    static IEnumerable<Type> FindPluginsInAssembly(Assembly assembly) => assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(DbiPlugin)));
}
