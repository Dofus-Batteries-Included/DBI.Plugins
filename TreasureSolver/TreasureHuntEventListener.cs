using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using DofusBatteriesIncluded.Core.Protocol;
using DofusBatteriesIncluded.TreasureSolver.Behaviours;

namespace DofusBatteriesIncluded.TreasureSolver;

public class TreasureHuntEventListener : IMessageListener<TreasureHuntEvent>, IMessageListener<TreasureHuntFinishedEvent>
{
    public Task HandleAsync(TreasureHuntEvent @event)
    {
        TreasureHuntManager.SetLastEvent(@event);
        return Task.CompletedTask;
    }

    public Task HandleAsync(TreasureHuntFinishedEvent message)
    {
        TreasureHuntManager.Finish();
        return Task.CompletedTask;
    }
}
