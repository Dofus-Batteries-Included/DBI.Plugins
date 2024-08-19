using System;
using System.Collections.Generic;
using System.Linq;
using DofusBatteriesIncluded.TreasureSolver.Models;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class StaticClueFinder : IClueFinder
{
    public readonly Dictionary<Position, IReadOnlyCollection<int>> Clues;

    public StaticClueFinder(Dictionary<Position, IReadOnlyCollection<int>> clues)
    {
        Clues = clues;
    }

    public Position? FindPositionOfNextClue(Position start, Direction direction, int clueId, int maxDistance)
    {
        foreach (Position position in GetPositionsInDirection(start, direction, maxDistance))
        {
            if (!Clues.TryGetValue(position, out IReadOnlyCollection<int> clues) || !clues.Contains(clueId))
            {
                continue;
            }

            return position;
        }

        return null;
    }

    static IEnumerable<Position> GetPositionsInDirection(Position start, Direction direction, int maxDistance)
    {
        for (int i = 1; i <= maxDistance; i++)
        {
            yield return direction switch
            {
                Direction.Left => start with { X = start.X - i },
                Direction.Right => start with { X = start.X + i },
                Direction.Top => start with { Y = start.Y - i },
                Direction.Bottom => start with { Y = start.Y + i },
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}
