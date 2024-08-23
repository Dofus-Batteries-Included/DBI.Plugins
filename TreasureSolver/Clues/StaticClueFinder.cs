using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core.Maps;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class StaticClueFinder : IClueFinder
{
    public readonly Dictionary<Position, IReadOnlyCollection<int>> Clues;

    public StaticClueFinder(Dictionary<Position, IReadOnlyCollection<int>> clues)
    {
        Clues = clues;
    }

    public Task<Position?> FindPositionOfNextClue(Position start, Direction direction, int clueId, int maxDistance)
    {
        foreach (Position position in GetPositionsInDirection(start, direction, maxDistance))
        {
            if (!Clues.TryGetValue(position, out IReadOnlyCollection<int> clues) || !clues.Contains(clueId))
            {
                continue;
            }

            return Task.FromResult<Position?>(position);
        }

        return Task.FromResult<Position?>(null);
    }

    static IEnumerable<Position> GetPositionsInDirection(Position start, Direction direction, int maxDistance)
    {
        for (int i = 1; i <= maxDistance; i++)
        {
            yield return start.MoveInDirection(direction, i);
        }
    }
}
