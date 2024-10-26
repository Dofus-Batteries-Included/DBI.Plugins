using BepInEx;
using BepInEx.Configuration;

namespace DBI.Hell.Configuration;

public class ConfigurationManager
{
    readonly ConfigFile _bepinexConfigFile;
    readonly List<Entry> _entries = [];

    internal ConfigurationManager(string fileName = "DofusBatteriesIncluded.cfg")
    {
        _bepinexConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, fileName), false);
    }

    public ConfigurationEntryBuilder<T> Configure<T>(string category, string key, T defaultValue) where T: IEquatable<T> => Configure(null, category, key, defaultValue);

    public ConfigurationEntryBuilder<T> Configure<T>(string pluginName, string category, string key, T defaultValue) where T: IEquatable<T> =>
        new(pluginName, category, key, defaultValue);

    public Entry GetEntry(string category, string key) => _entries.FirstOrDefault(e => e.Category == category && e.Key == key);
    public Entry GetEntry(string pluginName, string category, string key) => _entries.FirstOrDefault(e => e.PluginName == pluginName && e.Category == category && e.Key == key);

    public Entry<T> GetEntry<T>(string category, string key) => _entries.OfType<Entry<T>>().FirstOrDefault(e => e.Category == category && e.Key == key);

    public Entry<T> GetEntry<T>(string pluginName, string category, string key) =>
        _entries.OfType<Entry<T>>().FirstOrDefault(e => e.PluginName == pluginName && e.Category == category && e.Key == key);

    public IEnumerable<Entry> GetAll() => _entries;

    internal T Bind<T>(ConfigurationEntryBuilder<T> builder) where T: IEquatable<T>
    {
        Entry<T> entry = GetEntry<T>(builder.Category, builder.Key);
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
        entry = new Entry<T>(builder.PluginName, builder.Category, builder.Key, builder.DefaultValue, acceptableValues, bepinexEntry) { Hidden = builder.Hidden };
        _entries.Add(entry);

        foreach (ConfigurationEntryBuilder<T>.Callback callback in builder.Callbacks)
        {
            entry.ValueChanged += (_, newValue) => callback.OnValueChangedCallback(newValue);
            if (callback.CallWithInitialValue)
            {
                callback.OnValueChangedCallback(
                    new ConfigurationValueChangedArgs<T> { OldValue = entry.Value, NewValue = entry.Value, Source = ConfigurationChangeSource.Internal }
                );
            }
        }

        return entry.Value;
    }

    public abstract class Entry
    {
        internal Entry(string pluginName, string category, string key, Type type)
        {
            PluginName = pluginName;
            Category = category;
            Key = key;
            Type = type;
        }

        /// <summary>
        ///     Optional. The plugin that added this entry.
        /// </summary>
        public string PluginName { get; }

        public string Category { get; }
        public string Key { get; }
        public abstract ValueDescription CurrentValueDescription { get; }
        public abstract ValueDescription DefaultValueDescription { get; }
        public abstract IReadOnlyList<ValueDescription> AcceptableValuesDescriptions { get; }
        public Type Type { get; }
        public abstract ConfigDescription Description { get; }
        public bool Hidden { get; set; }

        public abstract void SetValueWithName(string valueName, ConfigurationChangeSource source);
    }

    public class Entry<T> : Entry
    {
        internal Entry(string pluginName, string category, string key, T defaultValue, ConfigEntry<T> configEntry) : this(pluginName, category, key, defaultValue, [], configEntry)
        {
        }

        internal Entry(string pluginName, string category, string key, T defaultValue, T[] acceptableValues, ConfigEntry<T> configEntry) : base(
            pluginName,
            category,
            key,
            typeof(T)
        )
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

        public event EventHandler<ConfigurationValueChangedArgs<T>> ValueChanged;

        public void SetValue(T value, ConfigurationChangeSource source)
        {
            T oldValue = ConfigEntry.Value;
            ConfigEntry.Value = value;
            ValueChanged?.Invoke(this, new ConfigurationValueChangedArgs<T> { OldValue = oldValue, NewValue = value, Source = source });
        }

        public override void SetValueWithName(string valueName, ConfigurationChangeSource source)
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

            SetValue(value, source);
        }
    }
}

public class ConfigurationValueChangedArgs<T>
{
    public T OldValue { get; init; }
    public T NewValue { get; init; }
    public ConfigurationChangeSource Source { get; init; }
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
