using System.Threading.Tasks;
using DofusBatteriesIncluded.Plugins.Core.Maps;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Clues;

public interface ICluesService
{
    Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int cluesMaxDistance);
    Task RegisterCluesAsync(long mapId, params ClueWithStatus[] clues);
}
