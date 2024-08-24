using System.Threading.Tasks;
using DofusBatteriesIncluded.Core.Maps;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public interface IClueFinder
{
    public Task<long?> FindMapOfNextClue(long startMapId, Direction direction, int clueId, int maxDistance);
}
