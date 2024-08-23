using System;
using System.Collections.Generic;

namespace DofusBatteriesIncluded.Core.Configuration;

public class ConfigurationEntryBuilder<T> where T: IEquatable<T>
{
    internal ConfigurationEntryBuilder(string category, string key, T defaultValue)
    {
        Category = category;
        Key = key;
        DefaultValue = defaultValue;
    }

    public string Category { get; }
    public string Key { get; }
    public T DefaultValue { get; }
    public string Description { get; private set; } = "";
    public List<T> PossibleValues { get; } = [];
    public bool Hidden { get; private set; }

    public ConfigurationEntryBuilder<T> WithDescription(string value)
    {
        Description = value;
        return this;
    }

    public ConfigurationEntryBuilder<T> WithPossibleValues(params T[] values)
    {
        PossibleValues.AddRange(values);
        return this;
    }

    public ConfigurationEntryBuilder<T> Hide()
    {
        Hidden = true;
        return this;
    }

    public T Bind() => DBI.Configuration.Bind(this);
}
