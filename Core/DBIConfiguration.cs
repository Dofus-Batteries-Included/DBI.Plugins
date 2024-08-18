using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
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

    public T Bind<T>(string category, string key, T defaultValue, string description) => Bind(category, key, defaultValue, new ConfigDescription(description));

    public T Bind<T>(string category, string key, T defaultValue, ConfigDescription description = null)
    {
        Entry<T> entry = Get<T>(category, key);
        if (entry != null)
        {
            return entry.Value;
        }

        ConfigEntry<T> bepinexEntry = _bepinexConfigFile.Bind(category, key, defaultValue, description);
        entry = new Entry<T>(category, key, defaultValue, bepinexEntry);
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
