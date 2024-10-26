namespace Heaven.Abstractions.Configuration;

public interface IPluginConfigurationEntryBuilder
{
}

public interface IPluginConfigurationEntryBuilder<in T> : IPluginConfigurationEntryBuilder where T: IEquatable<T>
{
    IPluginConfigurationEntryBuilder<T> WithDescription(string name);
    IPluginConfigurationEntryBuilder<T> WithDefaultValue(T value);
    IPluginConfigurationEntryBuilder<T> WithPossibleValues(params T[] values);
    IPluginConfigurationEntryBuilder<T> Hide();
}
