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

        DBI.Player.CurrentCharacter.SetCurrentMap(message.MapId);

        return Task.CompletedTask;
    }

    public Task HandleAsync(MapMovementEvent message)
    {
        if (!IsMessageForCurrentPlayer(message))
        {
            return Task.CompletedTask;
        }

        DBI.Player.CurrentCharacter.SetCurrentCellId(message.Cells.array.Last(c => c != default));

        return Task.CompletedTask;
    }

    static bool IsMessageForCurrentPlayer(MapCurrentEvent message) => DBI.Player.CurrentCharacter != null;
    static bool IsMessageForCurrentPlayer(MapMovementEvent message) => DBI.Player.CurrentCharacter != null && DBI.Player.CurrentCharacter.CharacterId == message.CharacterId;
}
