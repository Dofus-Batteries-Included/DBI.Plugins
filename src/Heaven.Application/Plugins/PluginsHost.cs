using DBI.Heaven.Application.Configuration;
using Heaven.Abstractions;
using MediatR;
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
            PluginInstance instance = new(plugin, configuration, _scope.ServiceProvider.GetRequiredService<IMediator>());

            pluginInstances.RegisterInstance(instance);

            try
            {
                await instance.StartAsync(cancellationToken);
            }
            catch (Exception exn)
            {
                logger.LogError(exn, "Could not start plugin {Plugin}.", instance);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (PluginInstance instance in pluginInstances.GetInstances())
        {
            try
            {
                await instance.StopAsync(cancellationToken);
            }
            catch (Exception exn)
            {
                logger.LogError(exn, "Could not stop plugin {Plugin}.", instance);
            }
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
