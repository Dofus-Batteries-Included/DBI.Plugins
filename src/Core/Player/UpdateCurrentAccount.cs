using System.Threading.Tasks;
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
