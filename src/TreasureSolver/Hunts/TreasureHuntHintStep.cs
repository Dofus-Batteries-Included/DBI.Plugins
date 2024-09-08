using System.Linq;
using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Common;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using DofusBatteriesIncluded.Plugins.Core;
using Microsoft.Extensions.Logging;
using Direction = DofusBatteriesIncluded.Plugins.Core.Maps.Direction;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Hunts;

public class TreasureHuntHintStep : ITreasureHuntClueStep
{
    static readonly ILogger Log = DBI.Logging.Create<TreasureHuntHintStep>();

    long? _npcMapId;

    public TreasureHuntHintStep(long lastMapId, Direction direction, int npcId)
    {
        LastMapId = lastMapId;
        Direction = direction;
        NpcId = npcId;
    }

    public long LastMapId { get; }
    public Direction Direction { get; }
    public int NpcId { get; }

    public Task<long?> FindNextMap() => Task.FromResult(_npcMapId);

    public void OnMapChanged(MapComplementaryInformationEvent evt)
    {
        long mapId = evt.MapId;
        foreach (ActorPositionInformation actor in evt.Actors.array.Take(evt.Actors.Count))
        {
            ActorPositionInformation.Types.ActorInformation.InformationOneofCase? actorCase = actor.ActorInformation?.InformationCase;
            if (actorCase != ActorPositionInformation.Types.ActorInformation.InformationOneofCase.RolePlayActor || actor.ActorInformation.RolePlayActor == null)
            {
                continue;
            }

            if (actor.ActorInformation.RolePlayActor.TreasureHuntNpcId == NpcId)
            {
                _npcMapId = mapId;
                Log.LogInformation("Found NPC {NpcId} in map {TargetMapId}.", NpcId, _npcMapId);
                return;
            }
        }
    }
}
