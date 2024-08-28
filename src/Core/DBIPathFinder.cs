using System.Collections.Generic;
using DofusBatteriesIncluded.Core.Maps.PathFinding;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBIPathFinder
{
    static AStar _cachedAStar;

    internal DBIPathFinder() { }

    public GamePath GetShortestPath(long source, IEnumerable<long> targets)
    {
        AStar aStar = GetAStar();
        GamePath shortest = null;

        foreach (long target in targets)
        {
            GamePath path = aStar.GetShortestPath(source, target);

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

    public GamePath GetShortestPath(long source, long target)
    {
        AStar aStar = GetAStar();
        return aStar.GetShortestPath(source, target);
    }

    static AStar GetAStar() => _cachedAStar ??= new AStar();
}
