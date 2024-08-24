using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Character.Management;
using DofusBatteriesIncluded.Core.Protocol;

namespace DofusBatteriesIncluded.Core.Player;

public class UpdateCurrentPlayer : IMessageListener<CharacterSelectionEvent>
{
    public Task HandleAsync(CharacterSelectionEvent message)
    {
        if (message.ResultCase != CharacterSelectionEvent.ResultOneofCase.Success || message.Success == null)
        {
            return Task.CompletedTask;
        }

        DBI.Player.SetCurrentPlayer(
            message.Success.Character.Id,
            message.Success.Character.CharacterBasicInformation.Name,
            message.Success.Character.CharacterBasicInformation.Level
        );
        return Task.CompletedTask;
    }
}
