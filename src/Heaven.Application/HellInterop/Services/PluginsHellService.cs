using DBI.Heaven.Application.Configuration;
using DBI.Heaven.Application.Plugins;
using DBI.Hell.HeavenInterop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Heaven.Abstractions;
using PluginConfiguration = DBI.Hell.HeavenInterop.PluginConfiguration;
using PluginConfigurationCategory = DBI.Heaven.Application.Configuration.PluginConfigurationCategory;
using PluginConfigurationEntry = DBI.Heaven.Application.Configuration.PluginConfigurationEntry;

namespace DBI.Heaven.Application.HellInterop.Services;

class PluginsHellService(PluginInstancesService plugins) : Hell.HeavenInterop.Plugins.PluginsBase
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

    static Plugin ConvertPlugin(PluginInstance instance) => new() { Info = ConvertInfo(instance.Plugin.Info), Configuration = ConvertConfiguration(instance.Configuration) };

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

    static Hell.HeavenInterop.PluginConfigurationCategory ConvertConfigurationCategory(PluginConfigurationCategory category)
    {
        Hell.HeavenInterop.PluginConfigurationCategory result = new() { Name = category.Name };

        foreach (PluginConfigurationEntry entry in category.GetEntries())
        {
            result.Entries.Add(ConvertConfigurationEntry(entry));
        }

        return result;
    }

    static Hell.HeavenInterop.PluginConfigurationEntry ConvertConfigurationEntry(PluginConfigurationEntry entry) =>
        entry switch
        {
            PluginConfigurationEntry<bool> boolEntry => new Hell.HeavenInterop.PluginConfigurationEntry
            {
                Name = entry.Name, Description = entry.Description, BoolEntry = new PluginConfigurationBoolEntry { DefaultValue = boolEntry.DefaultValue }
            },
            PluginConfigurationEntry<string> stringEntry => new Hell.HeavenInterop.PluginConfigurationEntry
            {
                Name = entry.Name,
                Description = entry.Description,
                StringEntry = new PluginConfigurationStringEntry { DefaultValue = stringEntry.DefaultValue, PossibleValues = { stringEntry.PossibleValues } }
            },
            _ => new Hell.HeavenInterop.PluginConfigurationEntry { Name = entry.Name, Description = entry.Description }
        };
}
