using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using DofusBatteriesIncluded.Plugins.Core.Configuration;
using Il2CppSystem.IO;

namespace DofusBatteriesIncluded.Plugins.Core;

// ReSharper disable once InconsistentNaming
public class DBIConfiguration
{
    readonly ConfigFile _bepinexConfigFile;
    readonly List<Entry> _entries = [];

    internal DBIConfiguration(string fileName = "DofusBatteriesIncluded.cfg")
    {
        _bepinexConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, fileName), false);
    }

    public ConfigurationEntryBuilder<T> Configure<T>(string category, string key, T defaultValue) where T: IEquatable<T> => new(category, key, defaultValue);

    public Entry<T> Get<T>(string category, string key) => _entries.OfType<Entry<T>>().FirstOrDefault(e => e.Category == category && e.Key == key);

    public IEnumerable<Entry> GetAll() => _entries;

    internal T Bind<T>(ConfigurationEntryBuilder<T> builder) where T: IEquatable<T>
    {
        Entry<T> entry = Get<T>(builder.Category, builder.Key);
        if (entry != null)
        {
            return entry.Value;
        }

        T[] acceptableValues = builder.PossibleValues.ToArray();

        ConfigEntry<T> bepinexEntry = _bepinexConfigFile.Bind(
            builder.Category,
            builder.Key,
            builder.DefaultValue,
            new ConfigDescription(builder.Description, builder.PossibleValues.Count == 0 ? null : new AcceptableValueList<T>(acceptableValues))
        );
        entry = new Entry<T>(builder.Category, builder.Key, builder.DefaultValue, acceptableValues, bepinexEntry) { Hidden = builder.Hidden };
        _entries.Add(entry);

        foreach (ConfigurationEntryBuilder<T>.Callback callback in builder.Callbacks)
        {
            entry.ValueChanged += (_, newValue) => callback.OnValueChangedCallback(newValue);
            if (callback.CallWithInitialValue)
            {
                callback.OnValueChangedCallback(entry.Value);
            }
        }

        return entry.Value;
    }

    public abstract class Entry
    {
        internal Entry(string category, string key, Type type)
        {
            Category = category;
            Key = key;
            Type = type;
        }

        public string Category { get; }
        public string Key { get; }
        public abstract ValueDescription CurrentValueDescription { get; }
        public abstract ValueDescription DefaultValueDescription { get; }
        public abstract IReadOnlyList<ValueDescription> AcceptableValuesDescriptions { get; }
        public Type Type { get; }
        public abstract ConfigDescription Description { get; }
        public bool Hidden { get; set; }

        public abstract void SetValueWithName(string valueName);
    }

    public class Entry<T> : Entry
    {
        internal Entry(string category, string key, T defaultValue, ConfigEntry<T> configEntry) : this(category, key, defaultValue, [], configEntry)
        {
        }

        internal Entry(string category, string key, T defaultValue, T[] acceptableValues, ConfigEntry<T> configEntry) : base(category, key, typeof(T))
        {
            DefaultValue = defaultValue;
            AcceptableValues = acceptableValues;
            ConfigEntry = configEntry;
        }

        public override ConfigDescription Description => ConfigEntry.Description;
        public T Value => ConfigEntry.Value;
        public override ValueDescription CurrentValueDescription => new(Value?.ToString(), Value?.ToString());
        public T DefaultValue { get; }
        public override ValueDescription DefaultValueDescription => new(DefaultValue?.ToString(), DefaultValue?.ToString());
        public IReadOnlyList<T> AcceptableValues { get; }
        public override IReadOnlyList<ValueDescription> AcceptableValuesDescriptions => AcceptableValues?.Select(v => new ValueDescription(v?.ToString(), v?.ToString())).ToArray();
        public ConfigEntry<T> ConfigEntry { get; }

        public event EventHandler<T> ValueChanged;

        public void Set(T value)
        {
            ConfigEntry.Value = value;
            ValueChanged?.Invoke(this, value);
        }

        public override void SetValueWithName(string valueName)
        {
            T value;
            if (AcceptableValues == null)
            {
                value = DefaultValue;
            }
            else
            {
                int? index = AcceptableValuesDescriptions.Select((v, i) => new { v.Name, Index = i }).FirstOrDefault(v => v.Name == valueName)?.Index;
                value = index.HasValue ? AcceptableValues[index.Value] : DefaultValue;
            }

            Set(value);
        }
    }
}

public class ValueDescription
{
    public ValueDescription(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }
    public string DisplayName { get; }
}
