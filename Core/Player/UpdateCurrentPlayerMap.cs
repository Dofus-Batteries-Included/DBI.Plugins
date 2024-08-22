using System.Linq;
using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using DofusBatteriesIncluded.Core.Protocol;

namespace DofusBatteriesIncluded.Core.Player;

public class UpdateCurrentPlayerMap : IMessageListener<MapCurrentEvent>, IMessageListener<MapMovementEvent>
{
    public Task HandleAsync(MapCurrentEvent message)
    {
        if (!IsMessageForCurrentPlayer(message))
        {
            return Task.CompletedTask;
        }

        DBI.Player.State.SetCurrentMap(message.MapId);

        return Task.CompletedTask;
    }

    public Task HandleAsync(MapMovementEvent message)
    {
        if (!IsMessageForCurrentPlayer(message))
        {
            return Task.CompletedTask;
        }

        DBI.Player.State.SetCurrentCellId(message.Cells.array.Last(c => c != default));

        return Task.CompletedTask;
    }

    static bool IsMessageForCurrentPlayer(MapCurrentEvent message) => DBI.Player.State != null;
    static bool IsMessageForCurrentPlayer(MapMovementEvent message) => DBI.Player.State != null && DBI.Player.State.CharacterId == message.CharacterId;
}
