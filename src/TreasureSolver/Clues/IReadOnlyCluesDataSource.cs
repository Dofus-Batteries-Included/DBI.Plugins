using System.Collections.Generic;
using System.Linq;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public interface IReadOnlyCluesDataSource
{
    IReadOnlyCollection<ClueRecord> GetRecords(long mapId);
}

public static class CluesDataSourceExtensions
{
    public static IReadOnlyCollection<int> GetCluesAt(this IReadOnlyCluesDataSource dataSource, long mapId)
    {
        IReadOnlyCollection<ClueRecord> allRecords = dataSource.GetRecords(mapId);
        return GetCluesFromRecords(allRecords);
    }

    public static IReadOnlyCollection<int> GetCluesAt(this IEnumerable<IReadOnlyCluesDataSource> dataSources, long mapId)
    {
        IEnumerable<ClueRecord> allRecords = dataSources.SelectMany(d => d.GetRecords(mapId));
        return GetCluesFromRecords(allRecords);
    }

    static int[] GetCluesFromRecords(IEnumerable<ClueRecord> allRecords) =>
        allRecords.GroupBy(r => r.ClueId).Select(g => g.OrderByDescending(r => r.RecordDate).First().ClueId).ToArray();
}
