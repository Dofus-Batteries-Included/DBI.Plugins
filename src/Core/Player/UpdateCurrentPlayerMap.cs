using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Plugins.Core.Protocol;

namespace DofusBatteriesIncluded.Plugins.Core.Player;

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

        DBI.Player.CurrentCharacter.SetCurrentCellId(message.Cells.Last(c => c != default));

        return Task.CompletedTask;
    }

    static bool IsMessageForCurrentPlayer(MapCurrentEvent message) => DBI.Player.CurrentCharacter != null;
    static bool IsMessageForCurrentPlayer(MapMovementEvent message) => DBI.Player.CurrentCharacter != null && DBI.Player.CurrentCharacter.CharacterId == message.CharacterId;
}

public class MapMovementEvent
{
    public IReadOnlyList<long> Cells { get; set; }
    public long CharacterId { get; set; }
}

public class MapCurrentEvent
{
    public long MapId { get; set; }
}
