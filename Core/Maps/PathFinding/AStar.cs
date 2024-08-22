using System.Collections.Generic;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using Microsoft.Extensions.Logging;
using Enumerable = System.Linq.Enumerable;

namespace DofusBatteriesIncluded.Core.Maps.PathFinding;

class AStar
{
    const int MaxIterations = 100000;
    readonly Dictionary<(long, long), GamePath> _knownPaths = new();
    static readonly ILogger Log = DBI.Logging.Create<AStar>();

    public GamePath GetShortestPath(long source, long target)
    {
        if (source == target)
        {
            return GamePath.Empty(source);
        }

        if (!_knownPaths.ContainsKey((source, target)))
        {
            ComputePath(source, target);
        }

        return _knownPaths[(source, target)];
    }

    void ComputePath(long source, long target)
    {
        MapPositions sourceMap = DataCenterModule.mapPositionsRoot.GetMapPositionById(source);
        MapPositions targetMap = DataCenterModule.mapPositionsRoot.GetMapPositionById(target);

        Log.LogDebug("Cache miss while computing path from {SourceMap} to {TargetMap}", sourceMap.name, targetMap.name);

        Dictionary<long, long> cameFrom = new();

        if (!Explore(sourceMap, targetMap, cameFrom))
        {
            _knownPaths.Add((source, target), null);
        }

        List<ChangeMapStep> result = new();

        long current = target;
        while (cameFrom.ContainsKey(current))
        {
            MapPositions currentMap = DataCenterModule.mapPositionsRoot.GetMapPositionById(current);

            long previous = cameFrom[current];
            MapPositions previousMap = DataCenterModule.mapPositionsRoot.GetMapPositionById(previous);

            Direction? direction = GamePathUtils.GetDirectionFromTo(previousMap.GetPosition(), currentMap.GetPosition());
            result.Add(new ChangeMapStep(direction ?? Direction.Unknown));

            current = previous;

            _knownPaths[(current, target)] = new GamePath(source, target, Enumerable.ToArray(Enumerable.Reverse(result)));
            _knownPaths[(target, current)] = new GamePath(target, current, Enumerable.ToArray(Enumerable.Select(result, step => new ChangeMapStep(step.Direction.Invert()))));
        }
    }

    static bool Explore(MapPositions sourceMap, MapPositions targetMap, IDictionary<long, long> cameFrom)
    {
        HashSet<uint> closed = new();
        Dictionary<uint, int> open = new();
        Dictionary<uint, int> openCosts = new();

        open[sourceMap.id] = sourceMap.DistanceTo(targetMap);
        openCosts[sourceMap.id] = 0;

        int iteration = 0;
        while (open.Count > 0 && iteration < MaxIterations)
        {
            uint currentMapId = Enumerable.MinBy(open, kv => kv.Value).Key;
            open.Remove(currentMapId);

            if (currentMapId == targetMap.id)
            {
                return true;
            }

            int currentCost = openCosts[currentMapId];

            foreach (uint neighborId in GetNeighbors(currentMapId))
            {
                if (closed.Contains(neighborId) || openCosts.TryGetValue(neighborId, out int neighborCost) && neighborCost < currentCost)
                {
                    continue;
                }

                MapPositions neighborMap = DataCenterModule.mapPositionsRoot.GetMapPositionById(neighborId);
                openCosts[neighborId] = currentCost + 1;
                open[neighborId] = currentCost + neighborMap.DistanceTo(targetMap);
                cameFrom[neighborId] = currentMapId;
            }

            closed.Add(currentMapId);

            iteration++;
        }

        if (iteration > MaxIterations)
        {
            Log.LogWarning("AStar ran out of juice");
        }

        return false;
    }

    static IEnumerable<uint> GetNeighbors(uint currentMapId)
    {
        MapScrollActions scrollActions = DataCenterModule.mapScrollActionsRoot.GetMapScrollActionById(currentMapId);
        if (scrollActions.bottomExists)
        {
            yield return (uint)scrollActions.bottomMapId;
        }
        if (scrollActions.topExists)
        {
            yield return (uint)scrollActions.topMapId;
        }
        if (scrollActions.leftExists)
        {
            yield return (uint)scrollActions.leftMapId;
        }
        if (scrollActions.rightExists)
        {
            yield return (uint)scrollActions.rightMapId;
        }
    }
}
