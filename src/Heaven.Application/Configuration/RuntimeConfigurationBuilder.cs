using Heaven.Abstractions.Configuration;

namespace DBI.Heaven.Application.Configuration;

class RuntimeConfigurationBuilder : IRuntimeConfigurationBuilder
{
    readonly List<RuntimeConfigurationCategoryBuilder> _categories = [];
    readonly IRuntimeConfigurationCategoryBuilder _defaultCategoryBuilder;

    public RuntimeConfigurationBuilder()
    {
        _defaultCategoryBuilder = AddCategory("");
    }

    public IRuntimeConfigurationCategoryBuilder AddCategory(string categoryName)
    {
        RuntimeConfigurationCategoryBuilder builder = new(categoryName);
        _categories.Add(builder);
        return builder;
    }

    public IRuntimeConfigurationEntryBuilder<T> AddEntry<T>(string entryName) where T: IEquatable<T> => _defaultCategoryBuilder.AddEntry<T>(entryName);

    public RuntimeConfiguration Build()
    {
        IReadOnlyList<RuntimeConfigurationCategory> categories = _categories.Select(c => c.Build()).ToArray();
        return new RuntimeConfiguration(categories);
    }
}
