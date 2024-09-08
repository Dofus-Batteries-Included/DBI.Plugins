using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.Core.Maps;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.TreasureSolver.Hunts;

public class TreasureHunt
{
    static readonly ILogger Log = DBI.Logging.Create<TreasureHunt>();
    ICluesService _cluesService;
    readonly List<ITreasureHuntClueStep> _steps = [];

    public TreasureHunt(ICluesService cluesService)
    {
        _cluesService = cluesService;
    }

    public int CurrentCheckPoint { get; private set; }
    public int TotalCheckPoints { get; private set; }

    public int CurrentStepIndex { get; private set; }
    public int TotalSteps { get; private set; }

    public bool IsOver { get; private set; }
    public ITreasureHuntClueStep CurrentStep { get; private set; }

    public async Task<long?> GetNextMap()
    {
        if (CurrentStep == null)
        {
            return null;
        }

        return await CurrentStep.FindNextMap();
    }

    public void HandleTreasureHuntEvent(TreasureHuntEvent evt)
    {
        CurrentCheckPoint = evt.CurrentCheckPoint;
        TotalCheckPoints = evt.TotalCheckPoint;
        CurrentStepIndex = evt.Flags.Count;
        TotalSteps = evt.TotalStepCount;
        IsOver = false;

        if (CurrentStepIndex == TotalSteps)
        {
            Log.LogInformation("Reached end of current checkpoint.");
            CurrentStep = null;
            return;
        }

        if (CurrentCheckPoint == TotalCheckPoints)
        {
            Log.LogInformation("Reached end of hunt.");
            IsOver = true;
            CurrentStep = null;
            return;
        }

        long lastMapId = evt.Flags.Count == 0 ? evt.StartMapId : evt.Flags.array[evt.Flags.Count - 1].MapId;
        TreasureHuntEvent.Types.TreasureHuntStep nextStep = evt.KnownSteps.array[evt.KnownSteps.Count - 1];

        switch (nextStep.StepCase)
        {
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToPoi:
            {
                if (nextStep.FollowDirectionToPoi == null)
                {
                    Log.LogWarning("Field {Field} of event is empty.", nameof(nextStep.FollowDirectionToPoi));
                    return;
                }

                int clueId = nextStep.FollowDirectionToPoi.PoiLabelId;

                Direction? direction = GetDirection(nextStep.FollowDirectionToPoi.Direction);
                if (!direction.HasValue)
                {
                    Log.LogWarning("Found invalid direction in event {Direction}.", nextStep.FollowDirectionToPoi.Direction);
                    return;
                }

                CurrentStep = FindClueStep(lastMapId, direction.Value, clueId);
                if (CurrentStep == null)
                {
                    CurrentStep = new TreasureHuntClueStep(_cluesService, lastMapId, direction.Value, clueId);
                    _steps.Add(CurrentStep);
                }

                break;
            }
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirectionToHint:
            {
                if (nextStep.FollowDirectionToHint == null)
                {
                    Log.LogWarning("Field {Field} of event is empty.", nameof(nextStep.FollowDirectionToHint));
                    return;
                }

                Direction? direction = GetDirection(nextStep.FollowDirectionToHint.Direction);
                if (!direction.HasValue)
                {
                    Log.LogWarning("Found invalid direction in event {Direction}.", nextStep.FollowDirectionToPoi.Direction);
                    return;
                }

                int npcId = nextStep.FollowDirectionToHint.NpcId;

                CurrentStep = FindHintStep(lastMapId, direction.Value, npcId);
                if (CurrentStep == null)
                {
                    CurrentStep = new TreasureHuntHintStep(lastMapId, direction.Value, npcId);
                    _steps.Add(CurrentStep);
                }

                break;
            }
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.FollowDirection:
            {
                if (nextStep.FollowDirection == null)
                {
                    Log.LogWarning("Field {Field} of event is empty.", nameof(nextStep.FollowDirection));
                    return;
                }

                Direction? direction = GetDirection(nextStep.FollowDirectionToPoi.Direction);
                if (!direction.HasValue)
                {
                    Log.LogWarning("Found invalid direction in event {Direction}.", nextStep.FollowDirectionToPoi.Direction);
                    return;
                }

                int distance = nextStep.FollowDirection.MapCount;

                CurrentStep = FindFollowDirectionStep(lastMapId, direction.Value, distance);
                if (CurrentStep == null)
                {
                    CurrentStep = new TreasureHuntFollowDirectionStep(lastMapId, direction.Value, distance);
                    _steps.Add(CurrentStep);
                }

                break;
            }
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.Fight:
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.None:
            case TreasureHuntEvent.Types.TreasureHuntStep.StepOneofCase.Dig:
            default:
                break;
        }
    }

    public void HandleMapChangedEvent(MapComplementaryInformationEvent evt)
    {
        foreach (ITreasureHuntClueStep step in _steps)
        {
            step.OnMapChanged(evt);
        }
    }

    public void SetCluesService(ICluesService cluesService)
    {
        _cluesService = cluesService;
        foreach (TreasureHuntClueStep step in _steps.OfType<TreasureHuntClueStep>())
        {
            step.SetCluesService(cluesService);
        }
    }

    TreasureHuntClueStep FindClueStep(long lastMapId, Direction direction, int clueId) =>
        _steps.OfType<TreasureHuntClueStep>().FirstOrDefault(s => s.LastMapId == lastMapId && s.Direction == direction && s.ClueId == clueId);

    TreasureHuntHintStep FindHintStep(long lastMapId, Direction direction, int npcId) =>
        _steps.OfType<TreasureHuntHintStep>().FirstOrDefault(s => s.LastMapId == lastMapId && s.Direction == direction && s.NpcId == npcId);

    TreasureHuntFollowDirectionStep FindFollowDirectionStep(long lastMapId, Direction direction, int distance) =>
        _steps.OfType<TreasureHuntFollowDirectionStep>().FirstOrDefault(s => s.LastMapId == lastMapId && s.Direction == direction && s.Distance == distance);

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
