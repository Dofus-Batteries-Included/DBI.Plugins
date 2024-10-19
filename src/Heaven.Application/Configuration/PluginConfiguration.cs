namespace DBI.Heaven.Application.Configuration;

class PluginConfiguration(IReadOnlyList<PluginConfigurationCategory> categories)
{
    readonly Dictionary<string, PluginConfigurationCategory> _categories = categories.ToDictionary(c => c.Name, c => c);

    public IEnumerable<PluginConfigurationCategory> GetCategories() => _categories.Values;
    public PluginConfigurationCategory? GetCategory(string name) => _categories.GetValueOrDefault(name);
    public PluginConfigurationEntry? GetEntry(string name) => _categories.GetValueOrDefault("")?.GetEntry(name);
    public PluginConfigurationEntry? GetEntry(string categoryName, string name) => _categories.GetValueOrDefault(categoryName)?.GetEntry(name);
}
