using Heaven.Abstractions.Configuration;

namespace DBI.Heaven.Application.Configuration;

class RuntimeConfigurationCategoryBuilder(string categoryName) : IRuntimeConfigurationCategoryBuilder
{
    readonly List<RuntimeConfigurationEntryBuilder> _entries = [];

    public IRuntimeConfigurationEntryBuilder<T> AddEntry<T>(string name) where T: IEquatable<T>
    {
        RuntimeConfigurationEntryBuilder<T> builder = new(name);
        _entries.Add(builder);
        return builder;
    }

    public RuntimeConfigurationCategory Build()
    {
        IReadOnlyList<RuntimeConfigurationEntry> entries = _entries.Select(e => e.Build()).ToArray();
        return new RuntimeConfigurationCategory(categoryName, entries);
    }
}
