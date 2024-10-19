namespace DBI.Heaven.Application.Plugins;

static class PluginsHostingExtensions
{
    public static void ConfigurePlugins(this IServiceCollection services)
    {
        services.Configure<PluginsOptions>(opt => opt.PluginTypes.Add(typeof(TestPlugin.TestPlugin)));
        services.AddSingleton<PluginInstancesService>();
        services.AddHostedService<PluginsHost>();
    }
}
