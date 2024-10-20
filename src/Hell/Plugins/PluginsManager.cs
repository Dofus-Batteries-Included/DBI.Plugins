using System.Collections.Concurrent;
using DBI.Hell.Configuration;
using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.Plugins;

public class PluginsManager
{
    static readonly ILogger Logger = Hell.Logging.Create<PluginsManager>();
    readonly ConcurrentDictionary<string, PluginInstance> _plugins = [];

    internal PluginsManager() { }

    public async Task LoadFromHeavenAsync()
    {
        Logger.LogInformation("Start loading plugins from Heaven...");

        HellHeavenInterop.Plugins.PluginsClient client = new(Hell.Heaven.Channel);
        GetPluginsResponse plugins = await client.GetPluginsAsync(new Empty());

        _plugins.Clear();

        foreach (Plugin plugin in plugins.Plugins)
        {
            PluginInstance instance = new(plugin);
            _plugins[plugin.Info.Name] = instance;
            Logger.LogInformation("Found plugin {Plugin}.", instance);

            // send initial configuration
            await SendPluginConfigurationToHeavenAsync(client, instance);

            // send configuration each time it changes
            instance.ConfigurationChanged += (_, source) =>
            {
                if (source == ConfigurationChangeSource.Heaven)
                {
                    // avoid infinite communication loop
                    return;
                }

                HellHeavenInterop.Plugins.PluginsClient c = new(Hell.Heaven.Channel);
                SendPluginConfigurationToHeavenAsync(c, instance).GetAwaiter().GetResult();
            };
        }

        Logger.LogInformation("Done loading plugins from Heaven.");

        Hell.BackgroundJobs.Start(RefreshPluginStatusesAsync, nameof(RefreshPluginStatusesAsync));
        Hell.BackgroundJobs.Start(RefreshPluginConfigurationAsync, nameof(RefreshPluginConfigurationAsync));
    }

    public IEnumerable<PluginInstance> GetPlugins() => _plugins.Values;

    async Task RefreshPluginStatusesAsync(CancellationToken cancellationToken)
    {
        HellHeavenInterop.Plugins.PluginsClient client = new(Hell.Heaven.Channel);
        AsyncServerStreamingCall<PluginStatusChangedStreamResponse> stream = client.GetPluginStatusChangedStream(new Empty(), cancellationToken: cancellationToken);

        Logger.LogInformation("Subscribing to plugin status changes from Heaven...");

        await stream.ResponseHeadersAsync;

        Logger.LogInformation("Subscribed to plugin status changes from Heaven.");

        try
        {
            await foreach (PluginStatusChangedStreamResponse response in stream.ResponseStream.ReadAllAsync(cancellationToken))
            {
                PluginInstance pluginInstance = _plugins.GetValueOrDefault(response.Name);
                if (pluginInstance == null)
                {
                    Logger.LogWarning("Received status update for unknown plugin {Plugin}.", response.Name);
                    continue;
                }

                pluginInstance.UpdateStatus(response.Status);
                Logger.LogInformation("Updated status of plugin {Plugin} to {StatusMessage}.", response.Name, response.Status.Message);
            }
        }
        catch (RpcException exn) when (exn.StatusCode == StatusCode.Cancelled)
        {
            Logger.LogWarning("Plugin status changes stream has been cancelled by remote.");
        }

        Logger.LogInformation("Plugin status changes from Heaven stopped.");
    }

    async Task RefreshPluginConfigurationAsync(CancellationToken cancellationToken)
    {
        HellHeavenInterop.Plugins.PluginsClient client = new(Hell.Heaven.Channel);
        AsyncServerStreamingCall<PluginConfigurationChangedStreamResponse> stream = client.GetPluginConfigurationChangedStream(new Empty(), cancellationToken: cancellationToken);

        Logger.LogInformation("Subscribing to plugin configuration changes from Heaven...");

        await stream.ResponseHeadersAsync;

        Logger.LogInformation("Subscribed to plugin configuration changes from Heaven.");

        try
        {
            await foreach (PluginConfigurationChangedStreamResponse response in stream.ResponseStream.ReadAllAsync(cancellationToken))
            {
                PluginInstance pluginInstance = _plugins.GetValueOrDefault(response.Name);
                if (pluginInstance == null)
                {
                    Logger.LogWarning("Received configuration update for unknown plugin {Plugin}.", response.Name);
                    continue;
                }

                pluginInstance.UpdateConfiguration(response.Configuration, ConfigurationChangeSource.Heaven);
                Logger.LogInformation("Updated configuration of plugin {Plugin}.", response.Name);
            }
        }
        catch (RpcException exn) when (exn.StatusCode == StatusCode.Cancelled)
        {
            Logger.LogWarning("Plugin configuration changes stream has been cancelled by remote.");
        }

        Logger.LogInformation("Plugin configuration changes from Heaven stopped.");
    }

    static async Task SendPluginConfigurationToHeavenAsync(HellHeavenInterop.Plugins.PluginsClient client, PluginInstance instance)
    {
        PluginConfigurationValues configuration = instance.ReadConfigurationValues();
        await client.ConfigurePluginAsync(new ConfigurePluginRequest { Name = instance.Plugin.Info.Name, Configuration = configuration });
        Logger.LogInformation("Sent new configuration of plugin {Plugin} to Heaven.", instance);
    }
}
