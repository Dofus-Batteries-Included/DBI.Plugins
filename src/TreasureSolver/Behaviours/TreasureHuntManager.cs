using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using Core.DataCenter;
using Core.DataCenter.Metadata.World;
using DofusBatteriesIncluded.Plugins.Core;
using DofusBatteriesIncluded.Plugins.Core.Maps;
using DofusBatteriesIncluded.Plugins.Core.Maps.PathFinding;
using DofusBatteriesIncluded.Plugins.TreasureSolver.Clues;
using DofusBatteriesIncluded.Plugins.TreasureSolver.Hunts;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Direction = DofusBatteriesIncluded.Plugins.Core.Maps.Direction;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Behaviours;

public class TreasureHuntManager : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<TreasureHuntManager>();
    const int CluesMaxDistance = 10;

    TreasureHuntEvent _lastHuntEvent;
    MapComplementaryInformationEvent _lastMapEvent;
    TreasureHunt _hunt;
    bool _needUpdate;

    void Awake()
    {
        TreasureSolver.CluesServiceChanged += (_, _) =>
        {
            if (_hunt == null)
            {
                return;
            }

            ICluesService service = TreasureSolver.GetCluesService();
            _hunt.SetCluesService(service);
            _needUpdate = true;
        };

        CorePlugin.UseScrollActionsChanged += (_, _) =>
        {
            if (_hunt == null)
            {
                return;
            }

            _needUpdate = true;
        };

        DBI.Messaging.GetListener<TreasureHuntEvent>().MessageReceived += (_, huntEvent) => HandleTreasureHuntEvent(huntEvent);
        DBI.Messaging.GetListener<TreasureHuntFinishedEvent>().MessageReceived += (_, _) => _hunt = null;
        DBI.Messaging.GetListener<MapComplementaryInformationEvent>().MessageReceived += (_, mapEvent) => HandleMapEvent(mapEvent);
    }

    void Start() => StartCoroutine(UpdateCoroutine().WrapToIl2Cpp());

    void HandleTreasureHuntEvent(TreasureHuntEvent huntEvent)
    {
        _lastHuntEvent = huntEvent;
        _needUpdate = true;
    }

    void HandleMapEvent(MapComplementaryInformationEvent mapEvent)
    {
        _lastMapEvent = mapEvent;
        _needUpdate = true;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if (!_needUpdate)
            {
                yield return null;
                continue;
            }

            if (_lastHuntEvent == null)
            {
                _needUpdate = false;
                yield return null;
                continue;
            }

            if (!TreasureHuntWindowAccessor.TryClear())
            {
                yield return null;
                continue;
            }

            if (_hunt == null)
            {
                ICluesService cluesService = TreasureSolver.GetCluesService();
                _hunt = new TreasureHunt(cluesService);
            }

            _hunt.HandleTreasureHuntEvent(_lastHuntEvent);
            _hunt.HandleMapChangedEvent(_lastMapEvent);

            if (_hunt.CurrentStep != null)
            {
                int step = _hunt.CurrentStepIndex;
                Task<long?> nextPositionTask = _hunt.GetNextMap();
                while (!nextPositionTask.IsCompleted)
                {
                    MarkLoading(step);
                    yield return new WaitForSecondsRealtime(0.5f);
                }

                long? nextPosition = nextPositionTask.Result;
                bool ok;
                int fuel = 100;
                do
                {
                    ok = nextPosition.HasValue
                        ? TryMarkNextPosition(step, _hunt.CurrentStep.LastMapId, _hunt.CurrentStep is TreasureHuntClueStep clueStep ? clueStep.ClueId : null, nextPosition.Value)
                        : TryMarkUnknownPosition(
                            step,
                            _hunt.CurrentStep.LastMapId,
                            _hunt.CurrentStep.Direction,
                            _hunt.CurrentStep is TreasureHuntHintStep ? "Keep looking..." : "Not found."
                        );
                    fuel--;
                    yield return null;
                } while (!ok && fuel > 0);
            }

            _needUpdate = false;
            yield return null;
        }
    }

    static bool TryMarkNextPosition(int step, long lastMapId, long? nextClueId, long targetMapId)
    {
        MapPositionsRoot mapPositionsRoot = DataCenterModule.GetDataRoot<MapPositionsRoot>();
        Position? targetPosition = mapPositionsRoot.GetMapPositionById(targetMapId)?.GetPosition();
        if (!targetPosition.HasValue)
        {
            return true;
        }

        Position? lastPosition = mapPositionsRoot.GetMapPositionById(lastMapId)?.GetPosition();

        if (nextClueId.HasValue)
        {
            Log.LogInformation("Found next clue {ClueId} at {Position}.", nextClueId.Value, targetPosition);
        }
        else
        {
            Log.LogInformation("Found next position: {Position}.", targetPosition);
        }

        string stepMessage = $"[{targetPosition.Value.X},{targetPosition.Value.Y}]";

        if (DBI.Player.CurrentCharacter != null)
        {
            long playerMapId = DBI.Player.CurrentCharacter.CurrentMapId;
            Position playerPosition = DBI.Player.CurrentCharacter.CurrentMapPosition;
            Position? targetMapPosition = mapPositionsRoot.GetMapPositionById(targetMapId)?.GetPosition();
            if (playerMapId == targetMapId || !CorePlugin.UseScrollActions && playerPosition == targetMapPosition)
            {
                stepMessage += " arrived";
            }
            else
            {
                GamePath path = DBI.PathFinder.GetShortestPath(playerMapId, targetMapId);
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
        Position? lastFlagMapPosition = DataCenterModule.GetDataRoot<MapPositionsRoot>().GetMapPositionById(lastFlagMapId)?.GetPosition();
        Position playerPosition = DBI.Player.CurrentCharacter.CurrentMapPosition;

        bool foundMapInPath = lastFlagMapPosition.HasValue && playerPosition == lastFlagMapPosition.Value;
        if (!foundMapInPath)
        {
            foreach (long map in MapUtils.MoveInDirection(lastFlagMapId, direction).Take(CluesMaxDistance))
            {
                Position? mapPosition = DataCenterModule.GetDataRoot<MapPositionsRoot>().GetMapPositionById(map)?.GetPosition();
                if (mapPosition.HasValue && mapPosition.Value == playerPosition)
                {
                    foundMapInPath = true;
                    break;
                }
            }
        }

        return foundMapInPath
            ? TreasureHuntWindowAccessor.TrySetStepAdditionalText(step, text)
            : TreasureHuntWindowAccessor.TrySetStepAdditionalText(step, "Player out of search area");
    }

    static int _loadingDots;

    static bool MarkLoading(int step)
    {
        _loadingDots = _loadingDots % 3 + 1;
        string dots = _loadingDots switch
        {
            1 => ".",
            2 => "..",
            _ => "..."
        };

        return TreasureHuntWindowAccessor.TrySetStepAdditionalText(step, $"Loading{dots}");
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
