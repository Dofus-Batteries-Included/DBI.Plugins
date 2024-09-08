using Com.Ankama.Dofus.Server.Connection.Protocol;

namespace DofusBatteriesIncluded.Plugins.Core.Player;

public class CurrentAccountState
{
    public CurrentAccountState(IdentificationResponse.Types.Success response)
    {
        AccountId = response.AccountId;
        AccountNickname = response.AccountNickname;
    }

    public long AccountId { get; private set; }
    public string AccountNickname { get; private set; }
}
