using DBI.Heaven.Application.Configuration;
using Heaven.Abstractions;
using Microsoft.Extensions.Options;

namespace DBI.Heaven.Application.Plugins;

class PluginsHost(IOptions<PluginsOptions> pluginsOptions, PluginInstancesService pluginInstances, IServiceScopeFactory scopeFactory) : IHostedService, IDisposable
{
    readonly IServiceScope _scope = scopeFactory.CreateScope();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (Type pluginType in pluginsOptions.Value.PluginTypes)
        {
            DbiPlugin plugin = InstantiatePlugin(_scope.ServiceProvider, pluginType);

            PluginConfigurationBuilder configurationBuilder = new();
            plugin.SetupConfiguration(configurationBuilder);

            PluginConfiguration configuration = configurationBuilder.Build();
            PluginInstance instance = new(plugin, configuration);

            pluginInstances.RegisterInstance(instance);

            await plugin.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (PluginInstance instance in pluginInstances.GetInstances())
        {
            await instance.Plugin.StopAsync(cancellationToken);
        }
    }

    static DbiPlugin InstantiatePlugin(IServiceProvider services, Type type) =>
        ActivatorUtilities.CreateInstance(services, type) as DbiPlugin ?? throw new InvalidOperationException($"Could not instantiate plugin {type}.");

    public void Dispose()
    {
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
