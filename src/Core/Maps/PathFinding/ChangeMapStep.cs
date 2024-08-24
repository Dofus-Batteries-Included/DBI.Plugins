namespace DofusBatteriesIncluded.Core.Maps.PathFinding;

public class ChangeMapStep : GamePathStep
{
    public ChangeMapStep(Direction direction)
    {
        Direction = direction;
    }

    public Direction Direction { get; }
}
