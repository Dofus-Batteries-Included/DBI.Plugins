namespace Heaven.Abstractions.Configuration;

public interface IRuntimeConfigurationCategoryBuilder
{
    IRuntimeConfigurationEntryBuilder<T> AddEntry<T>(string name) where T: IEquatable<T>;
}
