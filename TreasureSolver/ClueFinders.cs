using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.TreasureSolver;

public static class ClueFinders
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(ClueFinders));
    static readonly List<Entry> Entries = [];
    static string _defaultFinder;

    public static event EventHandler<string> DefaultFinderChanged;

    public static IReadOnlyCollection<Entry> Finders => Entries;

    public static void RegisterFinder(string name, string displayName, Func<Task<IClueFinder>> finderFactory, bool isDefault = false)
    {
        Entries.Add(new Entry(name, displayName, finderFactory));

        if (isDefault)
        {
            _defaultFinder = name;
        }
    }

    public static void SetDefaultFinder(string name)
    {
        _defaultFinder = name;
        Log.LogInformation("Default finder set to {Finder}.", _defaultFinder);
        DefaultFinderChanged?.Invoke(null, _defaultFinder);
    }

    public static async Task<IClueFinder> GetFinder(string name)
    {
        Entry entry = Entries.FirstOrDefault(e => e.Name == name);
        if (entry == null)
        {
            return null;
        }

        return await entry.GetInstance();
    }

    public static async Task<IClueFinder> GetDefaultFinder()
    {
        if (!string.IsNullOrEmpty(_defaultFinder))
        {
            IClueFinder finder = await GetFinder(_defaultFinder);
            if (finder != null)
            {
                return finder;
            }
        }

        Entry entry = Entries.FirstOrDefault();
        if (entry == null)
        {
            return null;
        }

        return await entry.GetInstance();
    }

    public class Entry
    {
        public Entry(string name, string displayName, Func<Task<IClueFinder>> finderFactory)
        {
            Name = name;
            DisplayName = displayName;
            Factory = finderFactory;
        }

        public string Name { get; }
        public string DisplayName { get; }
        public Func<Task<IClueFinder>> Factory { get; }
        public IClueFinder Instance { get; set; }

        public async Task<IClueFinder> GetInstance() => Instance ??= await Factory.Invoke();
    }
}
