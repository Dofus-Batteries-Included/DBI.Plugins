using DofusBatteriesIncluded.Core.Maps;

namespace DofusBatteriesIncluded.Core.Player;

public class CurrentPlayerState
{
    public CurrentPlayerState(long characterId, string name, int level)
    {
        CharacterId = characterId;
        Name = name;
        Level = level;
    }

    public long CharacterId { get; }
    public string Name { get; }
    public int Level { get; }
    public long CurrentMapId { get; set; }
    public Position CurrentMapPosition { get; set; }
    public long CurrentCellId { get; set; }
}
