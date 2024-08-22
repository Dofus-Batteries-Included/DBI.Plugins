using System;

namespace DofusBatteriesIncluded.Core.Maps;

public static class PositionExtensions
{
    public static Position MoveInDirection(this Position start, Direction direction, int distance) =>
        direction switch
        {
            Direction.Left => start with { X = start.X - distance },
            Direction.Right => start with { X = start.X + distance },
            Direction.Top => start with { Y = start.Y - distance },
            Direction.Bottom => start with { Y = start.Y + distance },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
}
