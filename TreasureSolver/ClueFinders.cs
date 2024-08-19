using System.Collections.Generic;
using System.Linq;
using DofusBatteriesIncluded.TreasureSolver.Clues;

namespace DofusBatteriesIncluded.TreasureSolver;

public static class ClueFinders
{
    static readonly List<Entry> _entries = [];
    static string _defaultFinder;

    public static IReadOnlyCollection<Entry> Finders => _entries;

    public static IClueFinder DefaultFinder =>
        string.IsNullOrEmpty(_defaultFinder) ? _entries.FirstOrDefault()?.Finder : GetFinder(_defaultFinder) ?? _entries.FirstOrDefault()?.Finder;

    public static void RegisterFinder(string name, IClueFinder finder) => _entries.Add(new Entry(name, finder));
    public static IClueFinder GetFinder(string name) => _entries.FirstOrDefault(e => e.Name == name)?.Finder;

    public class Entry
    {
        public Entry(string name, IClueFinder finder)
        {
            Name = name;
            Finder = finder;
        }

        public string Name { get; }
        public IClueFinder Finder { get; }
    }
}
