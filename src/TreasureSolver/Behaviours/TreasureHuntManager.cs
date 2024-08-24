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
using DofusBatteriesIncluded.Core.Maps.PathFinding;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Direction = DofusBatteriesIncluded.Core.Maps.Direction;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.TreasureSolver.Behaviours;

public class TreasureHuntManager : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<TreasureHuntManager>();
    const int CluesMaxDistance = 10;

    static TreasureHuntManager _instance;
    TreasureHuntEvent _lastEvent;
    Coroutine _coroutine;
    readonly Dictionary<int, long> _knownNpcMapsIds = [];
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
            _instance._knownNpcMapsIds.Clear();
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
        bool foundLookingForNpc = false;
        foreach (ActorPositionInformation actor in evt.Actors.array.Where(a => a != null))
        {
            ActorPositionInformation.Types.ActorInformation.Types.RolePlayActor.ActorOneofCase? actorCase = actor.ActorInformation?.RolePlayActor?.ActorCase;
            if (actorCase != ActorPositionInformation.Types.ActorInformation.Types.RolePlayActor.ActorOneofCase.NpcActor)
            {
                continue;
            }

            int npcId = actor.ActorInformation.RolePlayActor.NpcActor.NpcId;
            _knownNpcMapsIds[npcId] = mapId;

            foundLookingForNpc |= npcId == _lookingForNpcId;
        }

        if (foundLookingForNpc)
        {
            SetLastEvent(_lastEvent);
        }
    }

    IEnumerator HandleEvent(TreasureHuntEvent evt)
    {
        int tryCount = 0;
        while (tryCount < 100)
        {
            // ensure window exists and is accessible before handling treasure hunt events
            // also clear it so that event handling only has to set fields properly
            if (TreasureHuntWindowAccessor.TryClear())
            {
                if (evt.CurrentCheckPoint == evt.TotalCheckPoint)
                {
                    Log.LogInformation("Reached end of current checkpoint.");
                    yield break;
                }

                if (evt.Flags.Count == evt.TotalStepCount)
                {
                    Log.LogInformation("Reached end of hunt.");
                    yield break;
                }

                int step = evt.KnownSteps.Count - 1;
                long lastMapId = evt.Flags.array.All(f => f == null) ? evt.StartMapId : evt.Flags.array.Last(f => f != null).MapId;
                TreasureHuntEvent.Types.TreasureHuntStep nextStep = evt.KnownSteps.array.Last(s => s != null);

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
                            Log.LogWarning("Field {Field} of event is empty.", nameof(nextStep.FollowDirectionToPoi));
                            yield break;
                        }

                        int poiId = nextStep.FollowDirectionToPoi.PoiLabelId;
                        Direction? direction = GetDirection(nextStep.FollowDirectionToPoi.Direction);
                        if (!direction.HasValue)
                        {
                            Log.LogWarning("Found invalid direction in event {Direction}.", nextStep.FollowDirectionToPoi.Direction);
                            yield break;
                        }

                        Task<long?> cluePositionTask = clueFinder.FindMapOfNextClue(lastMapId, direction.Value, poiId, CluesMaxDistance);
                        yield return CoroutineExtensions.WaitForCompletion(cluePositionTask);
                        long? clueMapId = cluePositionTask.Result;

                        bool done = clueMapId.HasValue ? TryMarkNextPosition(step, lastMapId, clueMapId.Value) : TryMarkUnknownPosition(step, lastMapId, direction.Value);
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
                            Log.LogWarning("Field {Field} of event is empty.", nameof(nextStep.FollowDirectionToHint));
                            yield break;
                        }

                        Direction? direction = GetDirection(nextStep.FollowDirectionToPoi.Direction);
                        if (!direction.HasValue)
                        {
                            Log.LogWarning("Found invalid direction in event {Direction}.", nextStep.FollowDirectionToPoi.Direction);
                            yield break;
                        }

                        int npcId = nextStep.FollowDirectionToHint.NpcId;
                        _lookingForNpcId = npcId;

                        bool done = _knownNpcMapsIds.TryGetValue(npcId, out long npcMapId)
                            ? TryMarkNextPosition(step, lastMapId, npcMapId)
                            : TryMarkUnknownPosition(step, lastMapId, direction.Value, "Keep looking...");
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
                            Log.LogWarning("Field {Field} of event is empty.", nameof(nextStep.FollowDirection));
                            yield break;
                        }

                        Direction? direction = GetDirection(nextStep.FollowDirectionToPoi.Direction);
                        if (!direction.HasValue)
                        {
                            Log.LogWarning("Found invalid direction in event {Direction}.", nextStep.FollowDirectionToPoi.Direction);
                            yield break;
                        }

                        long targetMapId = MapUtils.MoveInDirection(lastMapId, direction.Value).Skip(nextStep.FollowDirection.MapCount - 1).FirstOrDefault();
                        if (targetMapId == 0)
                        {
                            if (TryMarkUnknownPosition(step, lastMapId, direction.Value))
                            {
                                yield break;
                            }
                        }
                        else
                        {
                            if (TryMarkNextPosition(step, lastMapId, targetMapId))
                            {
                                yield break;
                            }
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

                TreasureHuntWindowAccessor.TrySetStepAdditionalText(step, "Searching...");
            }

            tryCount++;
            yield return null;
        }
    }

    static bool TryMarkNextPosition(int step, long lastMapId, long targetMapId)
    {
        MapPositionsRoot mapPositionsRoot = DataCenterModule.GetDataRoot<MapPositionsRoot>();
        Position? targetPosition = mapPositionsRoot.GetMapPositionById(targetMapId)?.GetPosition();
        if (!targetPosition.HasValue)
        {
            return true;
        }

        Position? lastPosition = mapPositionsRoot.GetMapPositionById(lastMapId)?.GetPosition();

        Log.LogInformation("Found next clue at {Position}.", targetPosition);

        string stepMessage = $"[{targetPosition.Value.X},{targetPosition.Value.Y}]";

        if (DBI.Player.State != null)
        {
            if (DBI.Player.State.CurrentMapId == targetMapId)
            {
                stepMessage += " arrived";
            }
            else
            {
                Position playerPosition = DBI.Player.State.CurrentMapPosition;
                GamePath path = DBI.PathFinder.GetShortestPath(DBI.Player.State.CurrentMapId, targetMapId);
                int distance = path?.Count ?? targetPosition.Value.DistanceTo(playerPosition);

                stepMessage += lastPosition.HasValue
                               && GamePathUtils.GetDirectionFromTo(playerPosition, targetPosition.Value)
                               == GamePathUtils.GetDirectionFromTo(lastPosition.Value, targetPosition.Value)?.Invert()
                    ? $" {distance} maps back"
                    : $" {distance} maps";
            }
        }

        return TreasureHuntWindowAccessor.TrySetStepAdditionalText(step, stepMessage);
    }

    static bool TryMarkUnknownPosition(int step, long lastFlagMapId, Direction direction, string text = "Not found")
    {
        Position playerPosition = DBI.Player.State.CurrentMapPosition;
        bool foundMapInPath = false;
        foreach (long map in MapUtils.MoveInDirection(lastFlagMapId, direction).Take(CluesMaxDistance))
        {
            Position? mapPosition = DataCenterModule.GetDataRoot<MapPositionsRoot>().GetMapPositionById(map)?.GetPosition();
            if (mapPosition.HasValue && mapPosition.Value == playerPosition)
            {
                foundMapInPath = true;
            }
        }

        return foundMapInPath ? TreasureHuntWindowAccessor.TrySetStepAdditionalText(step, text) : TreasureHuntWindowAccessor.TrySetStepAdditionalText(step, "Player out of search area");
    }

    static Direction? GetDirection(Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction direction) =>
        direction switch
        {
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.East => Direction.Right,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.South => Direction.Bottom,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.West => Direction.Left,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.North => Direction.Top,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.SouthEast => null,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.SouthWest => null,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.NorthWest => null,
            Com.Ankama.Dofus.Server.Game.Protocol.Common.Direction.NorthEast => null,
            _ => null
        };
}
