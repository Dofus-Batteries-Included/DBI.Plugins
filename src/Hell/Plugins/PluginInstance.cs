using DBI.Hell.Configuration;
using DBI.HellHeavenInterop;

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

    public event EventHandler<ConfigurationChangeSource> ConfigurationChanged;

    public void UpdateStatus(PluginStatus status) => Status = status;

    public void UpdateConfiguration(PluginConfigurationValues configurationValues, ConfigurationChangeSource source)
    {
        foreach (PluginConfigurationCategoryValues category in configurationValues.Categories)
        foreach (PluginConfigurationEntryValue entry in category.Entries)
        {
            ConfigurationManager.Entry configEntry = Hell.Configuration.GetEntry(Plugin.Info.Name, category.Name, entry.Name);

            switch (entry.PluginConfigurationEntryValueCase)
            {
                case PluginConfigurationEntryValue.PluginConfigurationEntryValueOneofCase.BoolValue:
                    ((ConfigurationManager.Entry<bool>)configEntry).SetValue(entry.BoolValue, source);
                    break;
                case PluginConfigurationEntryValue.PluginConfigurationEntryValueOneofCase.StringValue:
                    ((ConfigurationManager.Entry<string>)configEntry).SetValue(entry.StringValue, source);
                    break;
            }
        }
    }

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

    void SetupConfiguration(Plugin plugin)
    {
        foreach (PluginConfigurationCategory category in plugin.Configuration.Categories)
        {
            foreach (PluginConfigurationEntry entry in category.Entries)
            {
                switch (entry.PluginConfigurationEntryCase)
                {
                    case PluginConfigurationEntry.PluginConfigurationEntryOneofCase.BoolEntry:
                        Hell.Configuration.Configure(plugin.Info.Name, category.Name, entry.Name, entry.BoolEntry.DefaultValue ?? false)
                            .WithDescription(entry.Description)
                            .RegisterChangeCallback(args => ConfigurationChanged?.Invoke(this, args.Source))
                            .Bind();
                        break;
                    case PluginConfigurationEntry.PluginConfigurationEntryOneofCase.StringEntry:
                        Hell.Configuration.Configure(plugin.Info.Name, category.Name, entry.Name, entry.StringEntry.DefaultValue)
                            .WithDescription(entry.Description)
                            .WithPossibleValues(entry.StringEntry.PossibleValues.ToArray())
                            .RegisterChangeCallback(args => ConfigurationChanged?.Invoke(this, args.Source))
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
