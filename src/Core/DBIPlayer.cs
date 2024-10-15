using System;
using DofusBatteriesIncluded.Plugins.Core.Player;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Plugins.Core;

// ReSharper disable once InconsistentNaming
public class DBIPlayer
{
    static readonly ILogger Log = DBI.Logging.Create<DBIMessaging>();

    internal DBIPlayer() { }

    public CurrentAccountState Account { get; private set; }
    public CurrentPlayerState CurrentCharacter { get; private set; }
    public event EventHandler<CurrentAccountState> AccountChanged;
    public event EventHandler<CurrentPlayerState> CurrentCharacterChangeStarted;
    public event EventHandler<CurrentPlayerState> CurrentCharacterChangeCompleted;

    internal void SetAccount(IdentificationResponse message)
    {
        if (message.ResultCase != IdentificationResponse.ResultOneofCase.Success || message.Success == null)
        {
            return;
        }

        Account = new CurrentAccountState(message.Success);
        Log.LogInformation("Account: {Name} ({Id})", Account.AccountNickname, Account.AccountId);

        AccountChanged?.Invoke(this, Account);
    }

    internal void SetCurrentCharacter(Character character)
    {
        CurrentCharacter = new CurrentPlayerState(character);
        Log.LogInformation("Character: {Name} ({Id})", CurrentCharacter.Name, CurrentCharacter.CharacterId);

        CurrentCharacterChangeStarted?.Invoke(this, CurrentCharacter);
    }

    internal void OnPlayerChangeCompleted() => CurrentCharacterChangeCompleted?.Invoke(this, CurrentCharacter);
}

public class IdentificationResponse
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
            public long AccountId { get; set; }
            public string AccountNickname { get; set; }
        }
    }
}
