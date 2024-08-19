using DofusBatteriesIncluded.TreasureSolver.Models;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public interface IClueFinder
{
    public Position? FindPositionOfNextClue(Position start, Direction direction, int clueId, int maxDistance);
}
