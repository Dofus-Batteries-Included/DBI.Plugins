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
        MapScrollActionsRoot mapScrollActionsRoot = DataCenterModule.GetDataRoot<MapScrollActionsRoot>();
        MapPositionsRoot mapPositionsRoot = DataCenterModule.GetDataRoot<MapPositionsRoot>();

        MapScrollActions scrollActions = mapScrollActionsRoot.GetMapScrollActionById(mapId);
        if (scrollActions != null)
        {
            if (scrollActions.bottomExists)
            {
                yield return scrollActions.bottomMapId;
            }
            if (scrollActions.topExists)
            {
                yield return scrollActions.topMapId;
            }
            if (scrollActions.leftExists)
            {
                yield return scrollActions.leftMapId;
            }
            if (scrollActions.rightExists)
            {
                yield return scrollActions.rightMapId;
            }
        }
        else
        {
            MapPositions map = mapPositionsRoot.GetMapPositionById(mapId);
            if (map == null)
            {
                Log.LogWarning("Could not find map {Map}.", mapId);
                yield break;
            }

            foreach (Direction direction in new[] { Direction.Bottom, Direction.Top, Direction.Left, Direction.Right })
            {
                Position next = map.GetPosition().MoveInDirection(direction);
                MapCoordinates coords = DataCenterModule.mapCoordinatesRoot.GetMapCoordinatesByCoords(next.X, next.Y);

                foreach (long neighborId in coords.mapIds)
                {
                    MapPositions nextMap = mapPositionsRoot.GetMapPositionById(neighborId);
                    if (nextMap == null || nextMap.worldMap != map.worldMap)
                    {
                        continue;
                    }

                    yield return neighborId;
                    break;
                }
            }
        }
    }

    public static IEnumerable<long> MoveInDirection(long startMapId, Direction direction)
    {
        MapScrollActionsRoot mapScrollActionsRoot = DataCenterModule.GetDataRoot<MapScrollActionsRoot>();
        MapPositionsRoot mapPositionsRoot = DataCenterModule.GetDataRoot<MapPositionsRoot>();

        long currentMap = startMapId;
        while (true)
        {
            long? nextMapId = null;

            MapScrollActions scrollActions = mapScrollActionsRoot.GetMapScrollActionById(currentMap);
            if (scrollActions != null)
            {
                nextMapId = GetNextMapFromScrollActions(direction, scrollActions);
            }
            else
            {
                MapPositions map = mapPositionsRoot.GetMapPositionById(currentMap);
                if (map == null)
                {
                    Log.LogWarning("Could not find map {Map}.", currentMap);
                    yield break;
                }

                Position next = map.GetPosition().MoveInDirection(direction);
                MapCoordinates coords = DataCenterModule.mapCoordinatesRoot.GetMapCoordinatesByCoords(next.X, next.Y);

                foreach (long mapId in coords.mapIds)
                {
                    MapPositions nextMap = mapPositionsRoot.GetMapPositionById(mapId);
                    if (nextMap == null || nextMap.worldMap != map.worldMap)
                    {
                        continue;
                    }

                    nextMapId = mapId;
                    break;
                }
            }

            if (nextMapId.HasValue)
            {
                yield return nextMapId.Value;
                currentMap = nextMapId.Value;
            }
            else
            {
                yield break;
            }
        }
    }

    static long? GetNextMapFromScrollActions(Direction direction, MapScrollActions scrollActions)
    {
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
            return null;
        }

        return direction switch
        {
            Direction.Right => scrollActions.topMapId,
            Direction.Bottom => scrollActions.bottomMapId,
            Direction.Left => scrollActions.leftMapId,
            Direction.Top => scrollActions.rightMapId,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}
