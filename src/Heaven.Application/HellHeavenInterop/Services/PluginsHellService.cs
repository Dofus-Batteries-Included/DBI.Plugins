using DBI.Heaven.Application.Configuration;
using DBI.Heaven.Application.Plugins;
using DBI.Heaven.Application.Plugins.Notifications;
using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Heaven.Abstractions;
using PluginConfiguration = DBI.HellHeavenInterop.PluginConfiguration;
using PluginConfigurationCategory = DBI.Heaven.Application.Configuration.PluginConfigurationCategory;
using PluginConfigurationEntry = DBI.Heaven.Application.Configuration.PluginConfigurationEntry;

namespace DBI.Heaven.Application.HellHeavenInterop.Services;

class PluginsHellService(PluginInstancesService plugins) : DBI.HellHeavenInterop.Plugins.PluginsBase
{
    readonly List<IServerStreamWriter<PluginStatusChangedStreamResponse>> _writers = [];

    public override Task<GetPluginsResponse> GetPlugins(Empty request, ServerCallContext context)
    {
        GetPluginsResponse result = new();

        foreach (PluginInstance plugin in plugins.GetInstances())
        {
            result.Plugins.Add(ConvertPlugin(plugin));
        }

        return Task.FromResult(result);
    }

    public override Task GetPluginStatusChangedStream(Empty request, IServerStreamWriter<PluginStatusChangedStreamResponse> serverStreamWriter, ServerCallContext context)
    {
        _writers.Add(serverStreamWriter);

        // the server should never close this stream
        return Task.Delay(Timeout.Infinite);
    }

    public async Task WriteStatusChange(PluginStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        PluginStatusChangedStreamResponse message = new()
        {
            Name = notification.Instance.Plugin.Info.Name,
            Status = notification.Instance.ToHellStatus()
        };

        foreach (IServerStreamWriter<PluginStatusChangedStreamResponse> writer in _writers)
        {
            await writer.WriteAsync(message, cancellationToken);
        }
    }

    static Plugin ConvertPlugin(PluginInstance instance) =>
        new()
        {
            Info = ConvertInfo(instance.Plugin.Info),
            Status = instance.ToHellStatus(),
            Configuration = ConvertConfiguration(instance.Configuration)
        };

    static PluginInfo ConvertInfo(DbiPluginInfo pluginInfo) =>
        new()
        {
            Name = pluginInfo.Name, DisplayName = pluginInfo.DisplayName, Version = pluginInfo.Version.ToString()
        };

    static PluginConfiguration ConvertConfiguration(Configuration.PluginConfiguration configuration)
    {
        PluginConfiguration result = new();

        foreach (PluginConfigurationCategory category in configuration.GetCategories())
        {
            result.Categories.Add(ConvertConfigurationCategory(category));
        }

        return result;
    }

    static DBI.HellHeavenInterop.PluginConfigurationCategory ConvertConfigurationCategory(PluginConfigurationCategory category)
    {
        DBI.HellHeavenInterop.PluginConfigurationCategory result = new() { Name = category.Name };

        foreach (PluginConfigurationEntry entry in category.GetEntries())
        {
            result.Entries.Add(ConvertConfigurationEntry(entry));
        }

        return result;
    }

    static DBI.HellHeavenInterop.PluginConfigurationEntry ConvertConfigurationEntry(PluginConfigurationEntry entry) =>
        entry switch
        {
            PluginConfigurationEntry<bool> boolEntry => new DBI.HellHeavenInterop.PluginConfigurationEntry
            {
                Name = entry.Name, Description = entry.Description, BoolEntry = new PluginConfigurationBoolEntry { DefaultValue = boolEntry.DefaultValue }
            },
            PluginConfigurationEntry<string> stringEntry => new DBI.HellHeavenInterop.PluginConfigurationEntry
            {
                Name = entry.Name,
                Description = entry.Description,
                StringEntry = new PluginConfigurationStringEntry { DefaultValue = stringEntry.DefaultValue, PossibleValues = { stringEntry.PossibleValues } }
            },
            _ => new DBI.HellHeavenInterop.PluginConfigurationEntry { Name = entry.Name, Description = entry.Description }
        };
}

static class PluginsMappingExtensions
{
    public static PluginStatus ToHellStatus(this PluginInstance instance) =>
        new()
        {
            State = instance.Status switch
            {
                PluginInstance.PluginNotStarted => PluginState.NotStarted,
                PluginInstance.PluginStarting => PluginState.NotStarted,
                PluginInstance.PluginFailedToStart => PluginState.Error,
                PluginInstance.PluginRunning => PluginState.Running,
                PluginInstance.PluginStopping => PluginState.Running,
                PluginInstance.PluginStopped => PluginState.NotStarted,
                PluginInstance.PluginCrashed => PluginState.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(instance.Status), instance.Status, null)
            },
            Message = instance.Status.Message
        };
}
