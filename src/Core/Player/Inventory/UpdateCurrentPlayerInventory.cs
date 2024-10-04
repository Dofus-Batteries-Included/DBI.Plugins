using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Common;
using Com.Ankama.Dofus.Server.Game.Protocol.Inventory;
using DofusBatteriesIncluded.Plugins.Core.Protocol;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Plugins.Core.Player;

public class UpdateCurrentPlayerInventory : IMessageListener<InventoryContentEvent>
{
    static readonly ILogger Log = DBI.Logging.Create<UpdateCurrentPlayerInventory>();
    public Task HandleAsync(InventoryContentEvent message)
    {
        List<ObjectItemInventory> items = new();
        foreach (var item in message.Objects.array)
        {
            items.Add(item);
        }

        DBI.Player.CurrentCharacter.Inventory.Items = items;
        Log.LogDebug("Updated current inventory with {Items} items.", items.Count);
        return Task.CompletedTask;
    }
}