using System;

namespace DofusBatteriesIncluded.TreasureSolver.Database;

public class Clue
{
    public Clue(long mapId, int clueId)
    {
        MapId = mapId;
        ClueId = clueId;
    }

    public Guid Id { get; set; }
    public long MapId { get; set; }
    public int ClueId { get; set; }
}
