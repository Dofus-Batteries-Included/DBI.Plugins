namespace DBI.Heaven.Application.Configuration;

public class RuntimeConfigurationCategory(string name, IReadOnlyList<RuntimeConfigurationEntry> entries)
{
    readonly IReadOnlyDictionary<string, RuntimeConfigurationEntry> _entries = entries.ToDictionary(e => e.Name, e => e);

    public string Name { get; } = name;

    public RuntimeConfigurationEntry? GetEntry(string name) => _entries.GetValueOrDefault(name);
}
