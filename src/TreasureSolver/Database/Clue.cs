using System;
using Xtensive.Orm;

namespace DofusBatteriesIncluded.TreasureSolver.Database;

[HierarchyRoot]
[Index(nameof(MapId), nameof(ClueId), Unique = true)]
public class Clue : Entity
{
    public Clue(long mapId, int clueId)
    {
        MapId = mapId;
        ClueId = clueId;
    }

    [Field]
    [Key]
    public Guid Id { get; set; }

    [Field]
    public long MapId { get; set; }

    [Field]
    public int ClueId { get; set; }
}
