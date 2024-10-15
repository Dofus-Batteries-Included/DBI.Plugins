using System.Threading.Tasks;
using DofusBatteriesIncluded.Plugins.Core.Protocol;

namespace DofusBatteriesIncluded.Plugins.Core.Player;

public class UpdateCurrentPlayer : IMessageListener<CharacterSelectionEvent>, IMessageListener<CharacterLoadingCompleteEvent>
{
    public Task HandleAsync(CharacterSelectionEvent message)
    {
        if (message.ResultCase != CharacterSelectionEvent.ResultOneofCase.Success || message.Success == null)
        {
            return Task.CompletedTask;
        }

        DBI.Player.SetCurrentCharacter(message.Success.Character);
        return Task.CompletedTask;
    }

    public Task HandleAsync(CharacterLoadingCompleteEvent message)
    {
        DBI.Player.OnPlayerChangeCompleted();
        return Task.CompletedTask;
    }
}

public class CharacterLoadingCompleteEvent
{
}

public class CharacterSelectionEvent
{
    public ResultOneofCase ResultCase { get; set; }
    public Types.Success Success { get; set; }

    public enum ResultOneofCase
    {
        Success
    }

    public class Types
    {
        public class Success
        {
            public Character Character { get; set; }
        }
    }
}
