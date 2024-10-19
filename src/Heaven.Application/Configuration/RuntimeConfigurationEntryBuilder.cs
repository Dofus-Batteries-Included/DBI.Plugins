using Heaven.Abstractions.Configuration;

namespace DBI.Heaven.Application.Configuration;

abstract class RuntimeConfigurationEntryBuilder
{
    public abstract RuntimeConfigurationEntry Build();
}

class RuntimeConfigurationEntryBuilder<T>(string entryName) : RuntimeConfigurationEntryBuilder, IRuntimeConfigurationEntryBuilder<T> where T: IEquatable<T>
{
    string? _description;
    List<T>? _possibleValues;
    T? _defaultValue;
    bool _hidden;

    public IRuntimeConfigurationEntryBuilder<T> WithDescription(string value)
    {
        _description = value;
        return this;
    }

    public IRuntimeConfigurationEntryBuilder<T> WithPossibleValues(params T[] values)
    {
        _possibleValues ??= [];
        _possibleValues.AddRange(values);
        return this;
    }

    public IRuntimeConfigurationEntryBuilder<T> WithDefaultValue(T value)
    {
        _defaultValue = value;
        return this;
    }

    public IRuntimeConfigurationEntryBuilder<T> Hide()
    {
        _hidden = true;
        return this;
    }

    public override RuntimeConfigurationEntry Build() =>
        new RuntimeConfigurationEntry<T>(entryName)
        {
            Description = _description,
            PossibleValues = _possibleValues,
            DefaultValue = _defaultValue,
            Hidden = _hidden
        };
}
