using System;
using System.Collections.Generic;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core.Maps;

public static class MapPositionsExtensions
{
    public static Position GetPosition(this MapPositions map) => new(map.posX, map.posY);
    public static int DistanceTo(this MapPositions map, MapPositions otherMap) => Math.Abs(map.posX - otherMap.posX) + Math.Abs(map.posY - otherMap.posY);
}

public static class MapUtils
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(MapUtils));

    public static IEnumerable<long> GetNeighbors(long mapId)
    {
        foreach (Direction direction in new[] { Direction.Bottom, Direction.Top, Direction.Left, Direction.Right })
        {
            if (!TryGetAdjacentMap(mapId, direction, out long? adjacentMap) || !adjacentMap.HasValue)
            {
                continue;
            }

            yield return adjacentMap.Value;
        }
    }

    public static IEnumerable<long> MoveInDirection(long startMapId, Direction direction)
    {
        long currentMap = startMapId;
        while (true)
        {
            if (TryGetAdjacentMap(currentMap, direction, out long? nextMapId) && nextMapId.HasValue)
            {
                yield return nextMapId.Value;
                currentMap = nextMapId.Value;
                continue;
            }

            yield break;
        }
    }

    static bool TryGetAdjacentMap(long mapId, Direction direction, out long? nextMapId) =>
        CorePlugin.UseScrollActions && TryGetAdjacentMapFromScrollActions(mapId, direction, out nextMapId) || TryGetAdjacentMapFromMapPositions(mapId, direction, out nextMapId);

    static bool TryGetAdjacentMapFromScrollActions(long mapId, Direction direction, out long? nextMapId)
    {
        MapScrollActions scrollActions = DataCenterModule.GetDataRoot<MapScrollActionsRoot>().GetMapScrollActionById(mapId);
        if (scrollActions == null)
        {
            nextMapId = null;
            return false;
        }

        bool hasNextMap = direction switch
        {
            Direction.Right => scrollActions.topExists,
            Direction.Bottom => scrollActions.bottomExists,
            Direction.Left => scrollActions.leftExists,
            Direction.Top => scrollActions.rightExists,
            _ => false
        };

        if (!hasNextMap)
        {
            nextMapId = null;
        }
        else
        {
            nextMapId = direction switch
            {
                Direction.Right => scrollActions.topMapId,
                Direction.Bottom => scrollActions.bottomMapId,
                Direction.Left => scrollActions.leftMapId,
                Direction.Top => scrollActions.rightMapId,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        return true;
    }

    static bool TryGetAdjacentMapFromMapPositions(long mapId, Direction direction, out long? nextMapId)
    {
        MapPositionsRoot mapPositionsRoot = DataCenterModule.GetDataRoot<MapPositionsRoot>();
        MapPositions map = mapPositionsRoot.GetMapPositionById(mapId);
        if (map == null)
        {
            Log.LogWarning("Could not find map {Map}.", mapId);
            nextMapId = null;
            return true;
        }

        Position next = map.GetPosition().MoveInDirection(direction);
        MapCoordinates coords = DataCenterModule.mapCoordinatesRoot.GetMapCoordinatesByCoords(next.X, next.Y);
        if (coords == null)
        {
            nextMapId = null;
            return true;
        }

        nextMapId = null;
        foreach (long neighborMapId in coords.mapIds)
        {
            MapPositions nextMap = mapPositionsRoot.GetMapPositionById(neighborMapId);
            if (nextMap == null || nextMap.worldMap != map.worldMap)
            {
                continue;
            }

            nextMapId = neighborMapId;
            break;
        }

        return true;
    }
}
