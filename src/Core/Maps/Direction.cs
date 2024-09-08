using System;

namespace DofusBatteriesIncluded.Plugins.Core.Maps;

public enum Direction
{
    Unknown = 0,
    Right = 1,
    BottomRight = 2,
    Bottom = 3,
    BottomLeft = 4,
    Left = 5,
    TopLeft = 6,
    Top = 7,
    TopRight = 8
}

public static class DirectionExtensions
{
    public static Direction Invert(this Direction direction) =>
        direction switch
        {
            Direction.Right => Direction.Left,
            Direction.BottomRight => Direction.TopLeft,
            Direction.Bottom => Direction.Top,
            Direction.BottomLeft => Direction.TopRight,
            Direction.Left => Direction.Right,
            Direction.TopLeft => Direction.BottomRight,
            Direction.Top => Direction.Bottom,
            Direction.TopRight => Direction.BottomLeft,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
}
