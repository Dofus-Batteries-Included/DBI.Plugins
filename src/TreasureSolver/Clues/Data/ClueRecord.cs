using System;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Clues.Data;

public class ClueRecord
{
    public int ClueId { get; init; }
    public bool WasFound { get; init; }
    public DateTime RecordDate { get; init; }
}
