using System;

namespace DofusBatteriesIncluded.Core.Maps.PathFinding;

public static class GamePathUtils
{
    public const string RightArrow = "→";
    public const string BottomRightArrow = "↘";
    public const string BottomArrow = "↓";
    public const string BottomLeftArrow = "↙";
    public const string LeftArrow = "←";
    public const string TopLeftArrow = "↖";
    public const string TopArrow = "↑";
    public const string TopRightArrow = "↗";
    public const string Target = "🎯";

    public static Direction? GetDirectionFromTo(Position from, Position to)
    {
        if (from.X == to.X && from.Y == to.Y)
        {
            return null;
        }

        int xDiff = to.X - from.X;
        int yDiff = to.Y - from.Y;

        double angle = Math.Atan2(yDiff, xDiff);

        return angle switch
        {
            > -Math.PI / 6 and < Math.PI / 6 => Direction.Right,
            >= Math.PI / 6 and <= Math.PI / 3 => Direction.BottomRight,
            > Math.PI / 3 and < 2 * Math.PI / 3 => Direction.Bottom,
            >= 2 * Math.PI / 3 and <= 5 * Math.PI / 6 => Direction.BottomLeft,
            > 5 * Math.PI / 6 or < -5 * Math.PI / 6 => Direction.Left,
            >= -5 * Math.PI / 6 and <= -2 * Math.PI / 3 => Direction.TopLeft,
            > -2 * Math.PI / 3 and < -Math.PI / 3 => Direction.Top,
            >= -Math.PI / 3 and <= -Math.PI / 6 => Direction.TopRight,
            _ => Direction.Unknown
        };
    }

    public static string ToArrow(this Direction direction) =>
        direction switch
        {
            Direction.Right => RightArrow,
            Direction.BottomRight => BottomRightArrow,
            Direction.Bottom => BottomArrow,
            Direction.BottomLeft => BottomLeftArrow,
            Direction.Left => LeftArrow,
            Direction.TopLeft => TopLeftArrow,
            Direction.Top => TopArrow,
            Direction.TopRight => TopRightArrow,
            _ => null
        };

    public static string ToDoubleSidedArrow(this Direction direction)
    {
        string directionStr = ToArrow(direction);
        if (directionStr == null)
        {
            return null;
        }

        Direction opposite = direction.Invert();
        string? oppositeStr = ToArrow(opposite);
        if (oppositeStr == null)
        {
            return null;
        }

        return direction > opposite ? $"{oppositeStr}{directionStr}" : $"{directionStr}{oppositeStr}";
    }
}
