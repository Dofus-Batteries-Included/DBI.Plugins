using System;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Core.Maps;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core.Player;

public class CurrentPlayerState
{
    static readonly ILogger Log = DBI.Logging.Create<UpdateCurrentPlayerMap>();

    public CurrentPlayerState(long characterId, string name, int level)
    {
        CharacterId = characterId;
        Name = name;
        Level = level;
    }

    public long CharacterId { get; }
    public string Name { get; }
    public int Level { get; }
    public long CurrentMapId { get; private set; }
    public Position CurrentMapPosition { get; private set; }
    public long CurrentCellId { get; private set; }

    public event EventHandler<Position> MapChanged;

    public void SetCurrentMap(long mapId)
    {
        CurrentMapId = mapId;
        CurrentMapPosition = DataCenterModule.GetDataRoot<MapPositionsRoot>().GetMapPositionById(mapId).GetPosition();

        Log.LogDebug("Current player changed map: {Position}", DBI.Player.State.CurrentMapPosition);
        MapChanged?.Invoke(this, CurrentMapPosition);
    }

    public void SetCurrentCellId(long cellId)
    {
        CurrentCellId = cellId;

        Log.LogDebug("Current player moved, new cell: {Position}", DBI.Player.State.CurrentCellId);
    }
}
