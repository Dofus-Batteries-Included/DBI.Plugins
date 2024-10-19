using DBI.Heaven.Application.Configuration;
using Heaven.Abstractions;
using Microsoft.Extensions.Options;

namespace DBI.Heaven.Application.Plugins;

class PluginsHost(
    IOptions<PluginsOptions> pluginsOptions,
    PluginInstancesService pluginInstances,
    IServiceScopeFactory scopeFactory,
    ILogger<PluginsHost> logger
) : IHostedService, IDisposable
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

            try
            {
                instance.Started = true;
                instance.FailedToStart = false;
                instance.FailedToStartReason = null;

                await plugin.StartAsync(cancellationToken);
            }
            catch (Exception exn)
            {
                logger.LogError(exn, "Could not start plugin {Plugin}.", plugin);
                instance.Started = false;
                instance.FailedToStart = true;
                instance.FailedToStartReason = exn.Message;
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (PluginInstance instance in pluginInstances.GetInstances())
        {
            instance.Started = false;
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
