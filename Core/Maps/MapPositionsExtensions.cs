using System;
using Core.DataCenter.Metadata.World;

namespace DofusBatteriesIncluded.Core.Maps;

public static class MapPositionsExtensions
{
    public static Position GetPosition(this MapPositions map) => new(map.posX, map.posY);
    public static int DistanceTo(this MapPositions map, MapPositions otherMap) => Math.Abs(map.posX - otherMap.posX) + Math.Abs(map.posY - otherMap.posY);
}
