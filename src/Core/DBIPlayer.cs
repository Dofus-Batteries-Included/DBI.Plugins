using System;
using DofusBatteriesIncluded.Core.Player;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBIPlayer
{
    public CurrentPlayerState State { get; private set; }
    public event EventHandler<CurrentPlayerState> PlayerChanged;

    public void SetCurrentPlayer(long characterId, string name, int level)
    {
        State = new CurrentPlayerState(characterId, name, level);
        PlayerChanged?.Invoke(this, State);
    }
}
