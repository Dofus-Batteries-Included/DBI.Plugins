using System.Threading.Tasks;
using DofusBatteriesIncluded.Core.Maps;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public interface ICluesService
{
    Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int cluesMaxDistance);
    Task RegisterCluesAsync(long mapId, params int[] clues);
}
