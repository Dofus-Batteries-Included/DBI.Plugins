using System.Threading.Tasks;
using Com.Ankama.Dofus.Server.Game.Protocol.Gamemap;
using DofusBatteriesIncluded.Plugins.Core.Maps;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Hunts;

public interface ITreasureHuntClueStep
{
    long LastMapId { get; }
    Direction Direction { get; }

    Task<long?> FindNextMap();
    void OnMapChanged(MapComplementaryInformationEvent evt);
}
