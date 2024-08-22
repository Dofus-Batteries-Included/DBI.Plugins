using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Coroutines;
using DofusBatteriesIncluded.Core.Maps;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.TreasureSolver.Behaviours;

public class TreasureHuntManager : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<TreasureHuntManager>();

    static TreasureHuntManager _instance;
    Coroutine _coroutine;

    public static void SetLastEvent(TreasureHuntEvent lastEvent)
    {
        if (!_instance)
        {
            return;
        }

        if (_instance._coroutine != null)
        {
            _instance.StopCoroutine(_instance._coroutine);
        }

        _instance._coroutine = _instance.StartCoroutine(HandleEvent(lastEvent).WrapToIl2Cpp());
    }

    public static void Finish() => SetLastEvent(null);

    void Awake() => _instance = this;

    static IEnumerator HandleEvent(TreasureHuntEvent lastEvent)
    {
        int tryCount = 0;
        while (tryCount < 100)
        {
            // ensure window exists and is accessible before handling treasure hunt events
            // also clear it so that event handling only has to set fields properly
            if (TreasureHuntWindowAccessor.TryClear())
            {
                if (lastEvent == null || lastEvent.CurrentCheckPoint == lastEvent.TotalCheckPoint || lastEvent.Flags.Count == lastEvent.TotalStepCount)
                {
                    yield break;
                }

                IReadOnlyList<Position> flags = GetKnownPoiPositions(lastEvent);
                Position lastKnownCoord = flags.Count == 0 ? GetMapPosition(lastEvent.StartMapId) : flags[^1];
                TreasureHuntEvent.Types.TreasureHuntStep nextStep = GetNextStep(lastEvent);

                Task<IClueFinder> clueFinderTask = ClueFinders.GetDefaultFinder();
                yield return CoroutineExtensions.WaitForCompletion(clueFinderTask);
                IClueFinder clueFinder = clueFinderTask.Result;

                if (clueFinder == null)
                {
                    Log.LogError("Could not find clue finder.");
                    yield break;
                }

                if (HandleStep(lastEvent, lastKnownCoord, nextStep, clueFinder))
                {
                    yield break;
                }

                TreasureHuntWindowAccessor.TrySetStepAdditionalText(lastEvent.KnownSteps.Count - 1, "Searching...");
            }

            tryCount++;
            yield return null;
        }
    }

    static bool HandleStep(TreasureHuntEvent message, Position startPosition, TreasureHuntEvent.Types.TreasureHuntStep step, IClueFinder clueFinder)
    {
        switch (step.StepCase)
        {
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToPoi:
            {
                if (step.FollowDirectionToPoi == null)
                {
                    return true;
                }

                int poiId = step.FollowDirectionToPoi.PoiLabelId;
                Direction direction = GetDirection(step.FollowDirectionToPoi.Direction);
                return MarkNextClue(message, startPosition, clueFinder, direction, poiId);
            }
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToHint:
                return true;
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirection:
            {
                if (step.FollowDirection == null)
                {
                    return true;
                }

                Direction direction = GetDirection(step.FollowDirectionToPoi.Direction);
                Position targetPosition = startPosition.MoveInDirection(direction, step.FollowDirection.MapCount);
                return TryMarkNextPosition(message, targetPosition);
            }
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.None:
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.Fight:
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.Dig:
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    static bool MarkNextClue(TreasureHuntEvent message, Position startPosition, IClueFinder clueFinder, Direction direction, int poiId)
    {
        Position? cluePosition = clueFinder.FindPositionOfNextClue(startPosition, direction, poiId, 10);
        return cluePosition.HasValue ? TryMarkNextPosition(message, cluePosition.Value) : TryMarkUnknownPosition(message);
    }

    static bool TryMarkNextPosition(TreasureHuntEvent message, Position position)
    {
        Log.LogInformation("Found next clue at {Position}.", position);
        return TreasureHuntWindowAccessor.TrySetStepAdditionalText(message.KnownSteps.Count - 1, $"[{position.X},{position.Y}]");
    }

    static bool TryMarkUnknownPosition(TreasureHuntEvent message)
    {
        Log.LogInformation("Could not find next clue.");
        return TreasureHuntWindowAccessor.TrySetStepAdditionalText(message.KnownSteps.Count - 1, "Not found");
    }

    static Position GetMapPosition(long id)
    {
        MapPositions map = DataCenterModule.mapPositionsRoot.GetMapPositionById(id);
        return new Position(map.posX, map.posY);
    }

    static IReadOnlyList<Position> GetKnownPoiPositions(TreasureHuntEvent message)
    {
        List<Position> result = [];
        foreach (TreasureHuntEvent.Types.TreasureHuntFlag flag in message.Flags.array.Where(f => f != null))
        {
            result.Add(GetMapPosition(flag.MapId));
        }
        return result;
    }

    static TreasureHuntEvent.Types.TreasureHuntStep GetNextStep(TreasureHuntEvent message) => message.KnownSteps.array.Last(s => s != null);

    static Direction GetDirection(Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction direction) =>
        direction switch
        {
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.East => Direction.Right,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.South => Direction.Bottom,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.West => Direction.Left,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.North => Direction.Top,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.SouthEast => throw new NotImplementedException(),
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.SouthWest => throw new NotImplementedException(),
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.NorthWest => throw new NotImplementedException(),
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.NorthEast => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
}
