using System.Threading.Tasks;
using DofusBatteriesIncluded.Core.Maps;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public interface IClueFinder
{
    public Task<Position?> FindPositionOfNextClue(Position start, Direction direction, int clueId, int maxDistance);
}
