using System.Collections.Concurrent;
using DBI.Hell.Configuration;
using DBI.Hell.Extensions;
using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.Plugins;

public class PluginInstance
{
    public PluginInstance(Plugin plugin)
    {
        Plugin = plugin;
        SetupConfiguration(plugin);
    }

    public Plugin Plugin { get; }
    public PluginStatus Status { get; private set; } = new() { State = PluginState.NotStarted };

    public void UpdateStatus(PluginStatus status) => Status = status;

    public PluginConfigurationValues ReadConfigurationValues()
    {
        PluginConfigurationValues configuration = new();

        IEnumerable<ConfigurationManager.Entry> entries = Hell.Configuration.GetAll().Where(e => e.PluginName == Plugin.Info.Name);
        foreach (IGrouping<string, ConfigurationManager.Entry> categoryGroup in entries.GroupBy(e => e.PluginName))
        {
            PluginConfigurationCategoryValues category = new();
            foreach (ConfigurationManager.Entry entry in categoryGroup)
            {
                PluginConfigurationEntryValue entryValue = entry switch
                {
                    ConfigurationManager.Entry<bool> boolEntry => new PluginConfigurationEntryValue
                    {
                        Name = entry.Key, BoolValue = boolEntry.Value
                    },
                    ConfigurationManager.Entry<string> stringEntry => new PluginConfigurationEntryValue
                    {
                        Name = entry.Key, StringValue = stringEntry.Value
                    },
                    _ => null
                };

                if (entryValue == null)
                {
                    continue;
                }

                category.Entries.Add(entryValue);
            }
        }

        return configuration;
    }

    public override string ToString() => $"{Plugin.Info.DisplayName} ({Plugin.Info.Name})";

    static void SetupConfiguration(Plugin plugin)
    {
        foreach (PluginConfigurationCategory category in plugin.Configuration.Categories)
        {
            foreach (PluginConfigurationEntry entry in category.Entries)
            {
                switch (entry.PluginConfigurationEntryCase)
                {
                    case PluginConfigurationEntry.PluginConfigurationEntryOneofCase.BoolEntry:
                        Hell.Configuration.Configure(plugin.Info.Name, category.Name, entry.Name, entry.BoolEntry.DefaultValue ?? false).WithDescription(entry.Description).Bind();
                        break;
                    case PluginConfigurationEntry.PluginConfigurationEntryOneofCase.StringEntry:
                        Hell.Configuration.Configure(plugin.Info.Name, category.Name, entry.Name, entry.StringEntry.DefaultValue)
                            .WithDescription(entry.Description)
                            .WithPossibleValues(entry.StringEntry.PossibleValues.ToArray())
                            .Bind();
                        break;
                    case PluginConfigurationEntry.PluginConfigurationEntryOneofCase.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(entry.PluginConfigurationEntryCase), entry.PluginConfigurationEntryCase, null);
                }
            }
        }
    }
}

public class PluginsManager
{
    static readonly ILogger Logger = Hell.Logging.Create<PluginsManager>();
    readonly ConcurrentDictionary<string, PluginInstance> _plugins = [];

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

            PluginConfigurationValues configuration = instance.ReadConfigurationValues();
            await client.ConfigurePluginAsync(new ConfigurePluginRequest { Name = plugin.Info.Name, Configuration = configuration });
            Logger.LogInformation("Updated configuration of plugin {Plugin}.", plugin);
        }

        Logger.LogInformation("Done loading plugins from Heaven.");

        RefreshPluginsAsync(client).Forget(Logger, nameof(RefreshPluginsAsync));
    }

    public IEnumerable<PluginInstance> GetPlugins() => _plugins.Values;

    async Task RefreshPluginsAsync(HellHeavenInterop.Plugins.PluginsClient client)
    {
        AsyncServerStreamingCall<PluginStatusChangedStreamResponse> stream = client.GetPluginStatusChangedStream(new Empty());

        Logger.LogInformation("Subscribing to plugin status changes from Heaven...");

        await stream.ResponseHeadersAsync;

        Logger.LogInformation("Subscribed to plugin status changes from Heaven.");

        try
        {
            await foreach (PluginStatusChangedStreamResponse response in stream.ResponseStream.ReadAllAsync())
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
}
