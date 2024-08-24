using System;

namespace DofusBatteriesIncluded.Core.Maps;

public record struct Position(int X, int Y);

public static class PositionExtensions
{
    public static Position MoveInDirection(this Position start, Direction direction, int distance) =>
        direction switch
        {
            Direction.Left => start with { X = start.X - distance },
            Direction.Right => start with { X = start.X + distance },
            Direction.Top => start with { Y = start.Y - distance },
            Direction.Bottom => start with { Y = start.Y + distance },
            Direction.BottomRight => new Position(start.X + distance, start.Y + distance),
            Direction.BottomLeft => new Position(start.X - distance, start.Y + distance),
            Direction.TopLeft => new Position(start.X + distance, start.Y - distance),
            Direction.TopRight => new Position(start.X - distance, start.Y - distance),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

    public static int DistanceTo(this Position from, Position to) => Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
}
