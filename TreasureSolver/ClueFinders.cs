using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.TreasureSolver.Clues;

namespace DofusBatteriesIncluded.TreasureSolver;

public static class ClueFinders
{
    static readonly List<Entry> Entries = [];
    static string _defaultFinder;

    public static IReadOnlyCollection<Entry> Finders => Entries;

    public static void RegisterFinder(string name, Func<Task<IClueFinder>> finderFactory, bool isDefault = false)
    {
        Entries.Add(new Entry(name, finderFactory));

        if (isDefault)
        {
            _defaultFinder = name;
        }
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
        public Entry(string name, Func<Task<IClueFinder>> finderFactory)
        {
            Name = name;
            Factory = finderFactory;
        }

        public string Name { get; }
        public Func<Task<IClueFinder>> Factory { get; }
        public IClueFinder Instance { get; set; }

        public async Task<IClueFinder> GetInstance() => Instance ??= await Factory.Invoke();
    }
}
