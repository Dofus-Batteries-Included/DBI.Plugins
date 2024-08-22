using System.Collections.Generic;

namespace DofusBatteriesIncluded.Core.Maps.PathFinding;

public static class PathFinder
{
    static readonly AStar AStar = new();

    public static GamePath GetShortestPath(long source, IEnumerable<long> targets)
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

    public static GamePath GetShortestPath(long source, long target) => AStar.GetShortestPath(source, target);
}
