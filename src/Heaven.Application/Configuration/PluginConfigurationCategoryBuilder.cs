using Heaven.Abstractions.Configuration;

namespace DBI.Heaven.Application.Configuration;

class PluginConfigurationCategoryBuilder(string categoryName) : IPluginConfigurationCategoryBuilder
{
    readonly List<RuntimeConfigurationEntryBuilder> _entries = [];

    public IPluginConfigurationEntryBuilder<T> AddEntry<T>(string name) where T: IEquatable<T>
    {
        PluginConfigurationEntryBuilder<T> builder = new(name);
        _entries.Add(builder);
        return builder;
    }

    public PluginConfigurationCategory Build()
    {
        IReadOnlyList<PluginConfigurationEntry> entries = _entries.Select(e => e.Build()).ToArray();
        return new PluginConfigurationCategory(categoryName, entries);
    }
}
