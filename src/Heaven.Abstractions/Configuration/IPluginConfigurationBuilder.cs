namespace Heaven.Abstractions.Configuration;

public interface IPluginConfigurationBuilder
{
    IPluginConfigurationCategoryBuilder AddCategory(string name);
    IPluginConfigurationEntryBuilder<T> AddEntry<T>(string name) where T: IEquatable<T>;
}
