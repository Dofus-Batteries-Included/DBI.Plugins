using System.Collections.Generic;
using DofusBatteriesIncluded.Core.Maps.PathFinding;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBIPathFinder
{
    static readonly AStar AStar = new();

    public GamePath GetShortestPath(long source, IEnumerable<long> targets)
    {
        GamePath shortest = null;

        foreach (long target in targets)
        {
            GamePath path = AStar.GetShortestPath(source, target);

            if (path == null)
            {
                continue;
            }

            if (shortest == null || path.Count < shortest.Count)
            {
                shortest = path;
            }
        }

        return shortest;
    }

    public GamePath GetShortestPath(long source, long target) => AStar.GetShortestPath(source, target);
}
