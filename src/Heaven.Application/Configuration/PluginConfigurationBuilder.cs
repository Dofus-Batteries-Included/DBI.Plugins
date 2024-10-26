using Heaven.Abstractions.Configuration;

namespace DBI.Heaven.Application.Configuration;

class PluginConfigurationBuilder : IPluginConfigurationBuilder
{
    readonly List<PluginConfigurationCategoryBuilder> _categories = [];
    readonly IPluginConfigurationCategoryBuilder _defaultCategoryBuilder;

    public PluginConfigurationBuilder()
    {
        _defaultCategoryBuilder = AddCategory("");
    }

    public IPluginConfigurationCategoryBuilder AddCategory(string categoryName)
    {
        PluginConfigurationCategoryBuilder builder = new(categoryName);
        _categories.Add(builder);
        return builder;
    }

    public IPluginConfigurationEntryBuilder<T> AddEntry<T>(string entryName) where T: IEquatable<T> => _defaultCategoryBuilder.AddEntry<T>(entryName);

    public PluginConfiguration Build()
    {
        IReadOnlyList<PluginConfigurationCategory> categories = _categories.Select(c => c.Build()).ToArray();
        return new PluginConfiguration(categories);
    }
}
