using System.Linq;
using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using Core.DataCenter;
using DofusBatteriesIncluded.Core.Maps;
using DofusBatteriesIncluded.Core.Protocol;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core.Player;

public class UpdateCurrentPlayerMap : IMessageListener<MapCurrentEvent>, IMessageListener<MapMovementEvent>
{
    static readonly ILogger Log = DBI.Logging.Create<UpdateCurrentPlayerMap>();

    public Task HandleAsync(MapCurrentEvent message)
    {
        if (!IsMessageForCurrentPlayer(message))
        {
            return Task.CompletedTask;
        }

        DBI.Player.State.CurrentMapId = message.MapId;
        DBI.Player.State.CurrentMapPosition = DataCenterModule.mapPositionsRoot.GetMapPositionById(message.MapId).GetPosition();

        Log.LogDebug("Current player changed map: {Position}", DBI.Player.State.CurrentMapPosition);

        return Task.CompletedTask;
    }

    public Task HandleAsync(MapMovementEvent message)
    {
        if (!IsMessageForCurrentPlayer(message))
        {
            return Task.CompletedTask;
        }

        DBI.Player.State.CurrentCellId = message.Cells.array.Last(c => c != default);

        Log.LogDebug("Current player moved, new cell: {Position}", DBI.Player.State.CurrentCellId);

        return Task.CompletedTask;
    }

    static bool IsMessageForCurrentPlayer(MapCurrentEvent message) => DBI.Player.State != null;
    static bool IsMessageForCurrentPlayer(MapMovementEvent message) => DBI.Player.State != null && DBI.Player.State.CharacterId == message.CharacterId;
}
