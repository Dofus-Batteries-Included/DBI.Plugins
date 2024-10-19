namespace Heaven.Abstractions.Configuration;

public interface IRuntimeConfigurationBuilder
{
    IRuntimeConfigurationCategoryBuilder AddCategory(string name);
    IRuntimeConfigurationEntryBuilder<T> AddEntry<T>(string name) where T: IEquatable<T>;
}
