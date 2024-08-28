using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Protocol;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver.Clues.Listeners;

public class SaveCluesOnDigAnswerEvent : IMessageListener<TreasureHuntEvent>, IMessageListener<TreasureHuntDigAnswerEvent>
{
    static readonly ILogger Log = DBI.Logging.Create<LocalCluesService>();
    TreasureHuntEvent _lastHuntEvent;

    public Task HandleAsync(TreasureHuntDigAnswerEvent message)
    {
        if (_lastHuntEvent is null)
        {
            Log.LogWarning("Could not find last hunt event, cannot extract data about correct and incorrect flags.");
            return Task.CompletedTask;
        }

        if (!TreasureSolver.TryGetCluesService(out ICluesService cluesService))
        {
            Log.LogWarning("Could not get clues service, skipping Treasure Hunt Event.");
            return Task.CompletedTask;
        }

        if (message.Result is TreasureHuntDigAnswerEvent.Types.DigResult.NewHint or TreasureHuntDigAnswerEvent.Types.DigResult.Finished)
        {
            for (int index = 0; index < _lastHuntEvent.Flags.Count; index++)
            {
                TreasureHuntEvent.Types.TreasureHuntStep step = _lastHuntEvent.KnownSteps[index];
                if (step.StepCase != TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToPoi || step.FollowDirectionToPoi == null)
                {
                    continue;
                }

                TreasureHuntEvent.Types.TreasureHuntFlag flag = _lastHuntEvent.Flags[index];

                cluesService.RegisterCluesAsync(flag.MapId, new ClueWithStatus(step.FollowDirectionToPoi.PoiLabelId, true));
            }
        }
        else if (message.Result is TreasureHuntDigAnswerEvent.Types.DigResult.Wrong or TreasureHuntDigAnswerEvent.Types.DigResult.WrongAndYouKnowIt
                 or TreasureHuntDigAnswerEvent.Types.DigResult.Lost)
        {
            for (int index = 0; index < _lastHuntEvent.Flags.Count; index++)
            {
                TreasureHuntEvent.Types.TreasureHuntStep step = _lastHuntEvent.KnownSteps[index];
                if (step.StepCase != TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToPoi || step.FollowDirectionToPoi == null)
                {
                    continue;
                }

                TreasureHuntEvent.Types.TreasureHuntFlag flag = _lastHuntEvent.Flags[index];
                if (flag.State == TreasureHuntEvent.Types.TreasureHuntFlag.Types.FlagState.Unknown)
                {
                    continue;
                }

                cluesService.RegisterCluesAsync(
                    flag.MapId,
                    new ClueWithStatus(step.FollowDirectionToPoi.PoiLabelId, flag.State == TreasureHuntEvent.Types.TreasureHuntFlag.Types.FlagState.Ok)
                );
            }
        }

        _lastHuntEvent = null;

        return Task.CompletedTask;
    }

    public Task HandleAsync(TreasureHuntEvent message)
    {
        if (_lastHuntEvent != null && message.CurrentCheckPoint != _lastHuntEvent.CurrentCheckPoint)
        {
            // sometimes dig events arrive after the hunt event of the next step
            // don't overwrite events of checkpoint until they are processed by the dig event handler
            return Task.CompletedTask;
        }

        _lastHuntEvent = message;
        return Task.CompletedTask;
    }
}
