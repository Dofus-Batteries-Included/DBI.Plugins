using System.Linq;
using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using DofusBatteriesIncluded.Plugins.Core.Maps;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Hunts;

public class TreasureHuntFollowDirectionStep : ITreasureHuntClueStep
{
    bool _targetMapHasBeenComputed;
    long? _targetMap;

    public TreasureHuntFollowDirectionStep(long lastMapId, Direction direction, int distance)
    {
        LastMapId = lastMapId;
        Direction = direction;
        Distance = distance;
    }

    public long LastMapId { get; }
    public Direction Direction { get; }
    public int Distance { get; }

    public Task<long?> FindNextMap()
    {
        if (!_targetMapHasBeenComputed)
        {
            _targetMap = MapUtils.MoveInDirection(LastMapId, Direction).Skip(Distance - 1).FirstOrDefault();
            _targetMapHasBeenComputed = true;
        }

        return Task.FromResult(_targetMap);
    }

    public void OnMapChanged(MapComplementaryInformationEvent evt) { }
}
