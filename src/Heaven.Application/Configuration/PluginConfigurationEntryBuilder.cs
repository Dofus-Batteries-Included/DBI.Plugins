using Heaven.Abstractions.Configuration;

namespace DBI.Heaven.Application.Configuration;

abstract class RuntimeConfigurationEntryBuilder
{
    public abstract PluginConfigurationEntry Build();
}

class PluginConfigurationEntryBuilder<T>(string entryName) : RuntimeConfigurationEntryBuilder, IPluginConfigurationEntryBuilder<T> where T: IEquatable<T>
{
    string? _description;
    List<T>? _possibleValues;
    T? _defaultValue;
    bool _hidden;

    public IPluginConfigurationEntryBuilder<T> WithDescription(string value)
    {
        _description = value;
        return this;
    }

    public IPluginConfigurationEntryBuilder<T> WithPossibleValues(params T[] values)
    {
        _possibleValues ??= [];
        _possibleValues.AddRange(values);
        return this;
    }

    public IPluginConfigurationEntryBuilder<T> WithDefaultValue(T value)
    {
        _defaultValue = value;
        return this;
    }

    public IPluginConfigurationEntryBuilder<T> Hide()
    {
        _hidden = true;
        return this;
    }

    public override PluginConfigurationEntry Build() =>
        new PluginConfigurationEntry<T>(entryName)
        {
            Description = _description,
            PossibleValues = _possibleValues,
            DefaultValue = _defaultValue,
            Hidden = _hidden
        };
}
