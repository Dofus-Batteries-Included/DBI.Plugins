using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using DofusBatteriesIncluded.Core.Configuration;
using Il2CppSystem.IO;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBIConfiguration
{
    readonly ConfigFile _bepinexConfigFile;
    readonly List<Entry> _entries = [];

    public DBIConfiguration(string fileName = "DofusBatteriesIncluded.cfg")
    {
        _bepinexConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, fileName), true);
    }

    public ConfigurationEntryBuilder<T> Configure<T>(string category, string key, T defaultValue) => new(category, key, defaultValue);

    internal T Bind<T>(ConfigurationEntryBuilder<T> builder)
    {
        Entry<T> entry = Get<T>(builder.Category, builder.Key);
        if (entry != null)
        {
            return entry.Value;
        }

        ConfigEntry<T> bepinexEntry = _bepinexConfigFile.Bind(builder.Category, builder.Key, builder.DefaultValue, builder.Description);
        entry = new Entry<T>(builder.Category, builder.Key, builder.DefaultValue, bepinexEntry) { Hidden = builder.Hidden };
        _entries.Add(entry);

        return entry.Value;
    }

    public Entry<T> Get<T>(string category, string key) => _entries.OfType<Entry<T>>().FirstOrDefault(e => e.Category == category && e.Key == key);

    public void Set<T>(string category, string key, T value)
    {
        Entry<T> entry = _entries.OfType<Entry<T>>().FirstOrDefault(e => e.Category == category && e.Key == key);
        if (entry == null)
        {
            throw new InvalidOperationException($"Entry {category}:{key} not found.");
        }

        entry.ConfigEntry.Value = value;
    }

    public IEnumerable<Entry> GetAll() => _entries;

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
        public Type Type { get; }
        public abstract ConfigDescription Description { get; }
        public bool Hidden { get; set; }
    }

    public class Entry<T> : Entry
    {
        internal Entry(string category, string key, T defaultValue, ConfigEntry<T> configEntry) : base(category, key, typeof(T))
        {
            DefaultValue = defaultValue;
            ConfigEntry = configEntry;
        }

        public override ConfigDescription Description => ConfigEntry.Description;
        public T Value => ConfigEntry.Value;
        public T DefaultValue { get; }
        public ConfigEntry<T> ConfigEntry { get; }
    }
}
