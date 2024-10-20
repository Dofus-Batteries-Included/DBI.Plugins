using System.Collections.Concurrent;
using DBI.Hell.Extensions;
using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DBI.Hell;

public class PluginsManager
{
    static readonly ILogger Logger = Hell.Logging.Create<PluginsManager>();
    readonly ConcurrentDictionary<string, Plugin> _plugins = [];

    public async Task LoadFromHeavenAsync()
    {
        Logger.LogInformation("Start loading plugins from Heaven...");

        Plugins.PluginsClient client = new(Hell.Heaven.Channel);
        GetPluginsResponse plugins = await client.GetPluginsAsync(new Empty());

        _plugins.Clear();

        foreach (Plugin plugin in plugins.Plugins)
        {
            _plugins[plugin.Info.Name] = plugin;
            Logger.LogInformation("Found plugin {DisplayName} ({Name}).", plugin.Info.DisplayName, plugin.Info.Name);
        }

        Logger.LogInformation("Done loading plugins from Heaven.");

        RefreshPluginsAsync(client).Forget(Logger, nameof(RefreshPluginsAsync));
    }

    public IEnumerable<Plugin> GetPlugins() => _plugins.Values;

    async Task RefreshPluginsAsync(Plugins.PluginsClient client)
    {
        AsyncServerStreamingCall<PluginStatusChangedStreamResponse> stream = client.GetPluginStatusChangedStream(new Empty());

        Logger.LogInformation("Subscribing to plugin status changes from Heaven...");

        await stream.ResponseHeadersAsync;

        Logger.LogInformation("Subscribed to plugin status changes from Heaven.");

        try
        {
            await foreach (PluginStatusChangedStreamResponse response in stream.ResponseStream.ReadAllAsync())
            {
                Plugin plugin = _plugins.GetValueOrDefault(response.Name);
                if (plugin == null)
                {
                    Logger.LogWarning("Received status update for unknown plugin {Plugin}.", response.Name);
                    continue;
                }

                plugin.Status = response.Status;
                Logger.LogInformation("Updated status of plugin {Plugin} to {StatusMessage}.", response.Name, response.Status.Message);
            }
        }
        catch (RpcException exn) when (exn.StatusCode == StatusCode.Cancelled)
        {
            Logger.LogWarning("Plugin status changes stream has been cancelled by remote.");
        }

        Logger.LogInformation("Plugin status changes from Heaven stopped.");
    }
}
