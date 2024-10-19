namespace Heaven.Abstractions.Configuration;

public interface IRuntimeConfigurationEntryBuilder
{
}

public interface IRuntimeConfigurationEntryBuilder<in T> : IRuntimeConfigurationEntryBuilder where T: IEquatable<T>
{
    IRuntimeConfigurationEntryBuilder<T> WithDescription(string name);
    IRuntimeConfigurationEntryBuilder<T> WithDefaultValue(T value);
    IRuntimeConfigurationEntryBuilder<T> WithPossibleValues(params T[] values);
    IRuntimeConfigurationEntryBuilder<T> Hide();
}
