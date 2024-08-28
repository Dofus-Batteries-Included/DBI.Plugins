using System;

namespace DofusBatteriesIncluded.TreasureSolver.Clues;

public class ClueRecord
{
    internal ClueRecord(int clueId, bool wasFound, DateTime recordDate)
    {
        ClueId = clueId;
        WasFound = wasFound;
        RecordDate = recordDate;
    }

    public int ClueId { get; set; }
    public bool WasFound { get; }
    public DateTime RecordDate { get; }
}
