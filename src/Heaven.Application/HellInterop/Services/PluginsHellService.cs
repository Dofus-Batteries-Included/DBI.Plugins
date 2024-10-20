using DBI.Heaven.Application.Configuration;
using DBI.Heaven.Application.GrpcUtils;
using DBI.Heaven.Application.Plugins;
using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Heaven.Abstractions;
using PluginConfiguration = DBI.HellHeavenInterop.PluginConfiguration;
using PluginConfigurationCategory = DBI.Heaven.Application.Configuration.PluginConfigurationCategory;
using PluginConfigurationEntry = DBI.Heaven.Application.Configuration.PluginConfigurationEntry;

namespace DBI.Heaven.Application.HellInterop.Services;

class PluginsHellService(PluginInstancesService plugins, ILoggerFactory loggerFactory) : HellHeavenInterop.Plugins.PluginsBase
{
    readonly ILogger<PluginsHellService> _logger = loggerFactory.CreateLogger<PluginsHellService>();

    public GrpcServerStreamingBroadcast<PluginStatusChangedStreamResponse> StatusBroadcast { get; } = new(
        loggerFactory.CreateLogger<GrpcServerStreamingBroadcast<PluginStatusChangedStreamResponse>>()
    );

    public GrpcServerStreamingBroadcast<PluginConfigurationChangedStreamResponse> ConfigurationBroadcast { get; } =
        new(loggerFactory.CreateLogger<GrpcServerStreamingBroadcast<PluginConfigurationChangedStreamResponse>>());

    public override Task<GetPluginsResponse> GetPlugins(Empty request, ServerCallContext context)
    {
        GetPluginsResponse result = new();

        foreach (PluginInstance plugin in plugins.GetInstances())
        {
            result.Plugins.Add(plugin.ToHellPlugin());
        }

        return Task.FromResult(result);
    }

    public override async Task<Empty> ConfigurePlugin(ConfigurePluginRequest request, ServerCallContext context)
    {
        PluginInstance? plugin = plugins.GetInstance(request.Name);
        if (plugin == null)
        {
            throw new InvalidOperationException($"Could not find plugin {request.Name}");
        }

        await plugin.UpdateConfigurationAsync(request.Configuration);
        _logger.LogInformation("Updated the configuration of plugin {Plugin}.", plugin);

        return new Empty();
    }

    public override async Task GetPluginStatusChangedStream(Empty request, IServerStreamWriter<PluginStatusChangedStreamResponse> serverStreamWriter, ServerCallContext context)
    {
        // start by writing the statuses of all the plugins now
        foreach (PluginInstance plugin in plugins.GetInstances())
        {
            PluginStatusChangedStreamResponse message = new()
            {
                Name = plugin.Plugin.Info.Name,
                Status = plugin.ToHellStatus()
            };

            await serverStreamWriter.WriteAsync(message);
        }

        // then append the writer to the writers os that next status changes are also written to this writer, see WriteStatusChangeAsync
        StatusBroadcast.RegisterWriter(serverStreamWriter);
        await StatusBroadcast.Run();
    }

    public override async Task GetPluginConfigurationChangedStream(
        Empty request,
        IServerStreamWriter<PluginConfigurationChangedStreamResponse> serverStreamWriter,
        ServerCallContext context
    )
    {
        ConfigurationBroadcast.RegisterWriter(serverStreamWriter);
        await ConfigurationBroadcast.Run();
    }
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

    public static Plugin ToHellPlugin(this PluginInstance instance) =>
        new()
        {
            Info = ToHellPluginInfo(instance.Plugin.Info),
            Configuration = ToHellPluginConfiguration(instance.Configuration)
        };

    public static PluginInfo ToHellPluginInfo(this DbiPluginInfo pluginInfo) =>
        new()
        {
            Name = pluginInfo.Name, DisplayName = pluginInfo.DisplayName, Version = pluginInfo.Version.ToString()
        };

    public static PluginConfiguration ToHellPluginConfiguration(this Configuration.PluginConfiguration configuration)
    {
        PluginConfiguration result = new();

        foreach (PluginConfigurationCategory category in configuration.GetCategories())
        {
            result.Categories.Add(ToHellPluginConfigurationCategory(category));
        }

        return result;
    }

    public static HellHeavenInterop.PluginConfigurationCategory ToHellPluginConfigurationCategory(this PluginConfigurationCategory category)
    {
        HellHeavenInterop.PluginConfigurationCategory result = new() { Name = category.Name };

        foreach (PluginConfigurationEntry entry in category.GetEntries())
        {
            result.Entries.Add(ToHellPluginConfigurationEntry(entry));
        }

        return result;
    }

    public static HellHeavenInterop.PluginConfigurationEntry ToHellPluginConfigurationEntry(this PluginConfigurationEntry entry) =>
        entry switch
        {
            PluginConfigurationEntry<bool> boolEntry => new HellHeavenInterop.PluginConfigurationEntry
            {
                Name = entry.Name, Description = entry.Description, BoolEntry = new PluginConfigurationBoolEntry { DefaultValue = boolEntry.DefaultValue }
            },
            PluginConfigurationEntry<string> stringEntry => new HellHeavenInterop.PluginConfigurationEntry
            {
                Name = entry.Name,
                Description = entry.Description,
                StringEntry = new PluginConfigurationStringEntry { DefaultValue = stringEntry.DefaultValue, PossibleValues = { stringEntry.PossibleValues } }
            },
            _ => new HellHeavenInterop.PluginConfigurationEntry { Name = entry.Name, Description = entry.Description }
        };

    public static PluginConfigurationValues ToHellPluginConfigurationValues(this Configuration.PluginConfiguration configuration)
    {
        PluginConfigurationValues result = new();

        foreach (PluginConfigurationCategory category in configuration.GetCategories())
        {
            result.Categories.Add(ToHellPluginConfigurationCategoryValues(category));
        }

        return result;
    }

    public static PluginConfigurationCategoryValues ToHellPluginConfigurationCategoryValues(this PluginConfigurationCategory category)
    {
        PluginConfigurationCategoryValues result = new() { Name = category.Name };

        foreach (PluginConfigurationEntry entry in category.GetEntries())
        {
            result.Entries.Add(ToHellPluginConfigurationEntryValue(entry));
        }

        return result;
    }

    public static PluginConfigurationEntryValue ToHellPluginConfigurationEntryValue(this PluginConfigurationEntry entry) =>
        entry switch
        {
            PluginConfigurationEntry<bool> boolEntry => new PluginConfigurationEntryValue
            {
                Name = entry.Name, Description = entry.Description, BoolValue = boolEntry.Value
            },
            PluginConfigurationEntry<string> stringEntry => new PluginConfigurationEntryValue
            {
                Name = entry.Name,
                Description = entry.Description,
                StringValue = stringEntry.Value ?? ""
            },
            _ => new PluginConfigurationEntryValue { Name = entry.Name, Description = entry.Description }
        };
}
