using System.Collections.Generic;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using Microsoft.Extensions.Logging;
using Enumerable = System.Linq.Enumerable;

namespace DofusBatteriesIncluded.Plugins.Core.Maps.PathFinding;

class AStar
{
    const int MaxIterations = 100000;
    static readonly ILogger Log = DBI.Logging.Create<AStar>();

    readonly MapPositionsRoot _mapPositionsRoot;
    MapPositions _sourceMap;
    readonly Dictionary<(long, long), GamePath> _knownPaths = new();

    public AStar()
    {
        _mapPositionsRoot = DataCenterModule.GetDataRoot<MapPositionsRoot>();
    }

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
        _sourceMap = _mapPositionsRoot.GetMapPositionById(source);
        if (_sourceMap == null)
        {
            _knownPaths.Add((source, target), null);
            return;
        }

        MapPositions targetMap = _mapPositionsRoot.GetMapPositionById(target);
        if (targetMap == null)
        {
            _knownPaths.Add((source, target), null);
            return;
        }

        Log.LogDebug("Cache miss while computing path from {SourceMap} to {TargetMap}", _sourceMap.GetPosition(), targetMap.GetPosition());

        Dictionary<long, long> cameFrom = new();

        if (!Explore(_sourceMap, targetMap, cameFrom))
        {
            _knownPaths.Add((source, target), null);
            return;
        }

        List<ChangeMapStep> result = new();

        long current = target;
        while (cameFrom.ContainsKey(current))
        {
            MapPositions currentMap = _mapPositionsRoot.GetMapPositionById(current);

            long previous = cameFrom[current];
            MapPositions previousMap = _mapPositionsRoot.GetMapPositionById(previous);

            Direction? direction = GamePathUtils.GetDirectionFromTo(previousMap.GetPosition(), currentMap.GetPosition());
            result.Add(new ChangeMapStep(direction ?? Direction.Unknown));

            current = previous;

            _knownPaths[(current, target)] = new GamePath(source, target, Enumerable.ToArray(Enumerable.Reverse(result)));
            _knownPaths[(target, current)] = new GamePath(target, current, Enumerable.ToArray(Enumerable.Select(result, step => new ChangeMapStep(step.Direction.Invert()))));
        }
    }

    bool Explore(MapPositions sourceMap, MapPositions targetMap, IDictionary<long, long> cameFrom)
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

            foreach (uint neighborId in MapUtils.GetNeighbors(currentMapId))
            {
                MapPositions neighborMap = DataCenterModule.GetDataRoot<MapPositionsRoot>().GetMapPositionById(neighborId);
                if (neighborMap == null)
                {
                    continue;
                }

                if (closed.Contains(neighborId) || openCosts.TryGetValue(neighborId, out int neighborCost) && neighborCost < currentCost)
                {
                    continue;
                }

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
}
