using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Com.Ankama.Dofus.Server.Game.Protocol.Common;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Coroutines;
using DofusBatteriesIncluded.Core.Maps;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Direction = DofusBatteriesIncluded.Core.Maps.Direction;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.TreasureSolver.Behaviours;

public class TreasureHuntManager : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<TreasureHuntManager>();

    static TreasureHuntManager _instance;
    TreasureHuntEvent _lastEvent;
    Coroutine _coroutine;
    readonly Dictionary<int, Position> _knownNpcPositions = [];
    int? _lookingForNpcId;

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

        _instance._lastEvent = lastEvent;

        if (lastEvent != null)
        {
            _instance._coroutine = _instance.StartCoroutine(_instance.HandleEvent(lastEvent).WrapToIl2Cpp());
        }
        else
        {
            _instance._knownNpcPositions.Clear();
        }
    }

    public static void Finish() => SetLastEvent(null);

    void Awake()
    {
        _instance = this;

        DBI.Player.PlayerChanged += (_, state) =>
        {
            state.MapChanged += (_, _) =>
            {
                if (_lastEvent != null)
                {
                    SetLastEvent(_lastEvent);
                }
            };
        };

        ClueFinders.DefaultFinderChanged += (_, _) =>
        {
            if (_lastEvent != null)
            {
                SetLastEvent(_lastEvent);
            }
        };

        DBI.Messaging.GetListener<MapComplementaryInformationEvent>().MessageReceived += (_, mapCurrent) => OnMapChanged(mapCurrent);
    }

    void OnMapChanged(MapComplementaryInformationEvent evt)
    {
        long mapId = evt.MapId;
        Position mapPosition = DataCenterModule.mapPositionsRoot.GetMapPositionById(mapId).GetPosition();
        bool foundLookingForNpc = false;
        foreach (ActorPositionInformation actor in evt.Actors.array.Where(a => a != null))
        {
            ActorPositionInformation.Types.ActorInformation.Types.RolePlayActor.ActorOneofCase? actorCase = actor.ActorInformation?.RolePlayActor?.ActorCase;
            if (actorCase != ActorPositionInformation.Types.ActorInformation.Types.RolePlayActor.ActorOneofCase.NpcActor)
            {
                continue;
            }

            int npcId = actor.ActorInformation.RolePlayActor.NpcActor.NpcId;
            _knownNpcPositions[npcId] = mapPosition;

            foundLookingForNpc |= npcId == _lookingForNpcId;
        }

        if (foundLookingForNpc)
        {
            SetLastEvent(_lastEvent);
        }
    }

    IEnumerator HandleEvent(TreasureHuntEvent lastEvent)
    {
        int tryCount = 0;
        while (tryCount < 100)
        {
            // ensure window exists and is accessible before handling treasure hunt events
            // also clear it so that event handling only has to set fields properly
            if (TreasureHuntWindowAccessor.TryClear())
            {
                if (lastEvent.CurrentCheckPoint == lastEvent.TotalCheckPoint || lastEvent.Flags.Count == lastEvent.TotalStepCount)
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

                _lookingForNpcId = null;
                switch (nextStep.StepCase)
                {
                    case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToPoi:
                    {
                        if (nextStep.FollowDirectionToPoi == null)
                        {
                            yield break;
                        }

                        int poiId = nextStep.FollowDirectionToPoi.PoiLabelId;
                        Direction direction = GetDirection(nextStep.FollowDirectionToPoi.Direction);

                        Task<Position?> cluePositionTask = clueFinder.FindPositionOfNextClue(lastKnownCoord, direction, poiId, 10);
                        yield return CoroutineExtensions.WaitForCompletion(cluePositionTask);
                        Position? cluePosition = cluePositionTask.Result;

                        bool done = cluePosition.HasValue ? TryMarkNextPosition(lastEvent, cluePosition.Value) : TryMarkUnknownPosition(lastEvent);
                        if (done)
                        {
                            yield break;
                        }

                        break;
                    }
                    case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToHint:
                    {
                        if (nextStep.FollowDirectionToHint == null)
                        {
                            yield break;
                        }

                        int npcId = nextStep.FollowDirectionToHint.NpcId;
                        _lookingForNpcId = npcId;

                        bool done = _knownNpcPositions.TryGetValue(npcId, out Position position) ? TryMarkNextPosition(lastEvent, position) : TryMarkUnknownNpcPosition(lastEvent);
                        if (done)
                        {
                            yield break;
                        }

                        yield break;
                    }
                    case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirection:
                    {
                        if (nextStep.FollowDirection == null)
                        {
                            yield break;
                        }

                        Direction direction = GetDirection(nextStep.FollowDirectionToPoi.Direction);
                        Position targetPosition = lastKnownCoord.MoveInDirection(direction, nextStep.FollowDirection.MapCount);
                        if (TryMarkNextPosition(lastEvent, targetPosition))
                        {
                            yield break;
                        }

                        break;
                    }
                    case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.None:
                    case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.Fight:
                    case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.Dig:
                        yield break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(nextStep.StepCase), nextStep.StepCase, null);
                }

                TreasureHuntWindowAccessor.TrySetStepAdditionalText(lastEvent.KnownSteps.Count - 1, "Searching...");
            }

            tryCount++;
            yield return null;
        }
    }

    static bool TryMarkNextPosition(TreasureHuntEvent message, Position position)
    {
        Log.LogInformation("Found next clue at {Position}.", position);

        string stepMessage = $"[{position.X},{position.Y}]";

        if (DBI.Player.State != null)
        {
            int distance = position.DistanceTo(DBI.Player.State.CurrentMapPosition);
            if (distance > 0)
            {
                stepMessage += $" {distance} maps";
            }
            else
            {
                stepMessage += " reached destination";
            }
        }

        return TreasureHuntWindowAccessor.TrySetStepAdditionalText(message.KnownSteps.Count - 1, stepMessage);
    }

    static bool TryMarkUnknownNpcPosition(TreasureHuntEvent message)
    {
        Log.LogInformation("NPC position is unknown.");
        return TreasureHuntWindowAccessor.TrySetStepAdditionalText(message.KnownSteps.Count - 1, "Not found yet");
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
