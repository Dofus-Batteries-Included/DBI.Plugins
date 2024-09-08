using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Connection.Protocol;
using DofusBatteriesIncluded.Plugins.Core.Protocol;

namespace DofusBatteriesIncluded.Plugins.Core.Player;

public class UpdateCurrentAccount : IMessageListener<IdentificationResponse>
{
    public Task HandleAsync(IdentificationResponse message)
    {
        DBI.Player.SetAccount(message);
        return Task.CompletedTask;
    }
}
