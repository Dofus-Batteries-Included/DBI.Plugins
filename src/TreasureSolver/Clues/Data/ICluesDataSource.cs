using System.Collections.Generic;
using System.Linq;

namespace DofusBatteriesIncluded.TreasureSolver.Clues.Data;

public interface ICluesDataSource
{
    IReadOnlyCollection<ClueRecord> GetRecords(long mapId);
    int MapsCount { get; }
    int CluesCount { get; }
    IReadOnlyDictionary<long, IReadOnlyCollection<ClueRecord>> Clues { get; }
}

public static class CluesDataSourceExtensions
{
    public static IReadOnlyCollection<int> GetCluesAt(this ICluesDataSource dataSource, long mapId)
    {
        IReadOnlyCollection<ClueRecord> allRecords = dataSource.GetRecords(mapId);
        return GetCluesFromRecords(allRecords);
    }

    public static IReadOnlyCollection<int> GetCluesAt(this IEnumerable<ICluesDataSource> dataSources, long mapId)
    {
        IEnumerable<ClueRecord> allRecords = dataSources.SelectMany(d => d.GetRecords(mapId));
        return GetCluesFromRecords(allRecords);
    }

    static int[] GetCluesFromRecords(IEnumerable<ClueRecord> allRecords) =>
        allRecords.GroupBy(r => r.ClueId).Select(g => g.OrderByDescending(r => r.RecordDate).First()).Where(c => c.WasFound).Select(c => c.ClueId).ToArray();
}
