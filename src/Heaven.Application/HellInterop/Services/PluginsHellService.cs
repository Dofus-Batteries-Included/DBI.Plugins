using DBI.Heaven.Application.Configuration;
using DBI.Heaven.Application.Plugins;
using DBI.HellHeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Heaven.Abstractions;
using PluginConfiguration = DBI.HellHeavenInterop.PluginConfiguration;
using PluginConfigurationCategory = DBI.Heaven.Application.Configuration.PluginConfigurationCategory;
using PluginConfigurationEntry = DBI.Heaven.Application.Configuration.PluginConfigurationEntry;

namespace DBI.Heaven.Application.HellInterop.Services;

class PluginsHellService(PluginInstancesService plugins) : HellHeavenInterop.Plugins.PluginsBase
{
    public override Task<GetPluginsResponse> GetPlugins(Empty request, ServerCallContext context)
    {
        GetPluginsResponse result = new();

        foreach (PluginInstance plugin in plugins.GetInstances())
        {
            result.Plugins.Add(ConvertPlugin(plugin));
        }

        return Task.FromResult(result);
    }

    static Plugin ConvertPlugin(PluginInstance instance) =>
        new()
        {
            Info = ConvertInfo(instance.Plugin.Info),
            Status = ConvertStatus(instance),
            Configuration = ConvertConfiguration(instance.Configuration)
        };

    static PluginInfo ConvertInfo(DbiPluginInfo pluginInfo) =>
        new()
        {
            Name = pluginInfo.Name, DisplayName = pluginInfo.DisplayName, Version = pluginInfo.Version.ToString()
        };

    static PluginStatus ConvertStatus(PluginInstance instance) =>
        new()
        {
            State = instance.FailedToStart
                ? PluginState.FailedToStart
                : instance.Started
                    ? PluginState.Running
                    : PluginState.NotStarted,
            FailedToStartReason = instance.FailedToStartReason ?? ""
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

    static HellHeavenInterop.PluginConfigurationCategory ConvertConfigurationCategory(PluginConfigurationCategory category)
    {
        HellHeavenInterop.PluginConfigurationCategory result = new() { Name = category.Name };

        foreach (PluginConfigurationEntry entry in category.GetEntries())
        {
            result.Entries.Add(ConvertConfigurationEntry(entry));
        }

        return result;
    }

    static HellHeavenInterop.PluginConfigurationEntry ConvertConfigurationEntry(PluginConfigurationEntry entry) =>
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
}
