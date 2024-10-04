using System.Collections.Generic;
using Com.Ankama.Dofus.Server.Game.Protocol.Common;

namespace DofusBatteriesIncluded.Plugins.Core.Player;

public class CurrentPlayerInventory
{

    public List<ObjectItemInventory> Items { get; set; } = new();
}