﻿using System;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Plugins.Core.Maps;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Plugins.Core.Player;

public class CurrentPlayerState
{
    static readonly ILogger Log = DBI.Logging.Create<CurrentPlayerState>();

    public CurrentPlayerState(Character character)
    {
        CharacterId = character.Id;
        Name = character.Name;
        Level = character.Level;
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

        Log.LogDebug("Current player changed map: map {MapId} at {Position}", DBI.Player.CurrentCharacter.CurrentMapId, DBI.Player.CurrentCharacter.CurrentMapPosition);
        MapChanged?.Invoke(this, CurrentMapPosition);
    }

    public void SetCurrentCellId(long cellId)
    {
        CurrentCellId = cellId;

        Log.LogDebug("Current player moved, new cell: {Position}", DBI.Player.CurrentCharacter.CurrentCellId);
    }
}
