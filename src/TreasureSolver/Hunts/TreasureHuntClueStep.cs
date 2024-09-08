using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using DofusBatteriesIncluded.Core;
using DofusBatteriesIncluded.TreasureSolver.Clues;
using Microsoft.Extensions.Logging;
using Direction = DofusBatteriesIncluded.Core.Maps.Direction;

namespace DofusBatteriesIncluded.TreasureSolver.Hunts;

public class TreasureHuntClueStep : ITreasureHuntClueStep
{
    static readonly ILogger Log = DBI.Logging.Create<TreasureHuntClueStep>();
    const int CluesMaxDistance = 10;

    ICluesService _cluesService;
    bool _targetMapHasBeenComputed;
    long? _targetMap;

    public TreasureHuntClueStep(ICluesService cluesService, long lastMapId, Direction direction, int clueId)
    {
        _cluesService = cluesService;
        LastMapId = lastMapId;
        Direction = direction;
        ClueId = clueId;
    }

    public long LastMapId { get; }
    public Direction Direction { get; }
    public int ClueId { get; }


    public async Task<long?> FindNextMap()
    {
        if (!_targetMapHasBeenComputed)
        {
            _targetMap = await _cluesService.FindMapOfNextClue(LastMapId, Direction, ClueId, CluesMaxDistance);
            _targetMapHasBeenComputed = true;

            if (_targetMap.HasValue)
            {
                Log.LogInformation("Looking for clue {ClueId} from map {MapId} looking {Direction}: found in map {TargetMapId}.", ClueId, LastMapId, Direction, _targetMap.Value);
            }
            else
            {
                Log.LogWarning("Looking for clue {ClueId} from map {MapId} looking {Direction}: not found.", ClueId, LastMapId, Direction);
            }
        }

        return _targetMap;
    }

    public void OnMapChanged(MapComplementaryInformationEvent evt) { }

    public void SetCluesService(ICluesService cluesService)
    {
        _cluesService = cluesService;
        _targetMapHasBeenComputed = false;
        _targetMap = null;
    }
}
