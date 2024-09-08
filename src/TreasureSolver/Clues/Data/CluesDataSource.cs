using System.Collections.Generic;
using System.Linq;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Clues.Data;

public class CluesDataSource : ICluesDataSource
{
    readonly Dictionary<long, List<ClueRecord>> _clues;

    public int MapsCount => _clues.Count;
    public int CluesCount => _clues.Sum(kv => kv.Value.Count);
    public IReadOnlyDictionary<long, IReadOnlyCollection<ClueRecord>> Clues => _clues.ToDictionary(kv => kv.Key, IReadOnlyCollection<ClueRecord> (kv) => kv.Value);

    public CluesDataSource(IReadOnlyDictionary<long, IReadOnlyCollection<ClueRecord>> clues = null)
    {
        _clues = clues?.ToDictionary(kv => kv.Key, kv => kv.Value.ToList()) ?? [];
    }

    public IReadOnlyCollection<ClueRecord> GetRecords(long mapId) => _clues.GetValueOrDefault(mapId) ?? [];

    public void AddRecords(long mapId, params ClueRecord[] records)
    {
        if (!_clues.ContainsKey(mapId))
        {
            _clues[mapId] = records.ToList();
        }
        else
        {
            _clues[mapId] = _clues[mapId].Concat(records).GroupBy(r => r.ClueId).Select(g => g.OrderByDescending(r => r.RecordDate).First()).ToList();
        }
    }
}
