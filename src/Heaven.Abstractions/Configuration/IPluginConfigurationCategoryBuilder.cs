namespace Heaven.Abstractions.Configuration;

public interface IPluginConfigurationCategoryBuilder
{
    IPluginConfigurationEntryBuilder<T> AddEntry<T>(string name) where T: IEquatable<T>;
}
