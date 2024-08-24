using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Core.Maps;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class StaticClueFinder : IClueFinder
{
    public readonly Dictionary<Position, IReadOnlyCollection<int>> Clues;

    public StaticClueFinder(Dictionary<Position, IReadOnlyCollection<int>> clues)
    {
        Clues = clues;
    }

    public Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int maxDistance)
    {
        foreach (long mapId in MapUtils.MoveInDirection(startMapId, direction).Take(maxDistance))
        {
            Position? mapPosition = DataCenterModule.GetDataRoot<MapPositionsRoot>().GetMapPositionById(mapId)?.GetPosition();
            if (!mapPosition.HasValue)
            {
                continue;
            }

            if (!Clues.TryGetValue(mapPosition.Value, out IReadOnlyCollection<int> clues) || !clues.Contains(clueId))
            {
                continue;
            }

            return Task.FromResult<long?>(mapId);
        }

        return Task.FromResult<long?>(null);
    }
}
