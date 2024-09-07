using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Connection.Protocol;
using DofusBatteriesIncluded.Core.Protocol;

namespace DofusBatteriesIncluded.Core.Player;

public class UpdateCurrentAccount : IMessageListener<IdentificationResponse>
{
    public Task HandleAsync(IdentificationResponse message)
    {
        DBI.Player.SetAccount(message);
        return Task.CompletedTask;
    }
}
