namespace DBI.Heaven.Application.Configuration;

public class PluginConfigurationCategory(string name, IReadOnlyList<PluginConfigurationEntry> entries)
{
    readonly IReadOnlyDictionary<string, PluginConfigurationEntry> _entries = entries.ToDictionary(e => e.Name, e => e);

    public string Name { get; } = name;

    public IEnumerable<PluginConfigurationEntry> GetEntries() => _entries.Values;
    public PluginConfigurationEntry? GetEntry(string name) => _entries.GetValueOrDefault(name);
}
