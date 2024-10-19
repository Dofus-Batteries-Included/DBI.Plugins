namespace DBI.Heaven.Application.Configuration;

class RuntimeConfiguration(IReadOnlyList<RuntimeConfigurationCategory> categories)
{
    readonly Dictionary<string, RuntimeConfigurationCategory> _categories = categories.ToDictionary(c => c.Name, c => c);

    public RuntimeConfigurationCategory? GetCategory(string name) => _categories.GetValueOrDefault(name);
    public RuntimeConfigurationEntry? GetEntry(string name) => _categories.GetValueOrDefault("")?.GetEntry(name);
    public RuntimeConfigurationEntry? GetEntry(string categoryName, string name) => _categories.GetValueOrDefault(categoryName)?.GetEntry(name);
}
