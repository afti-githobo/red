using Godot;
using System;

namespace Red
{
    [Flags]
    public enum Cardinal
    {
        None = 0,
        W = 1,
        N = 1 << 1,
        E = 1 << 3,
        S = 1 << 4,
        Vertical = N | S,
        Horizontal = E | W,
        NE = N | E,
        NW = N | W,
        SE = S | E,
        SW = S | W
    }

    public static partial class Extensions
    {
        public static Vector2 Vector(this Cardinal c)
        {
            switch (c)
            {
                case Cardinal.N:
                    return Vector2.Up;
                case Cardinal.S:
                    return Vector2.Down;
                case Cardinal.W:
                    return Vector2.Left;
                case Cardinal.E:
                    return Vector2.Right;
                case Cardinal.NW:
                    return (Vector2.Up + Vector2.Left).Normalized();
                case Cardinal.NE:
                    return (Vector2.Up + Vector2.Right).Normalized();
                case Cardinal.SW:
                    return (Vector2.Down + Vector2.Left).Normalized();
                case Cardinal.SE:
                    return (Vector2.Down + Vector2.Right).Normalized();
                default:
                    return Vector2.Zero;
            }
        }

        public static Vector2I VectorI(this Cardinal c)
        {
            switch (c)
            {
                case Cardinal.N:
                    return Vector2I.Up;
                case Cardinal.S:
                    return Vector2I.Down;
                case Cardinal.W:
                    return Vector2I.Left;
                case Cardinal.E:
                    return Vector2I.Right;
                case Cardinal.NW:
                    return Vector2I.Up + Vector2I.Left;
                case Cardinal.NE:
                    return Vector2I.Up + Vector2I.Right;
                case Cardinal.SW:
                    return Vector2I.Down + Vector2I.Left;
                case Cardinal.SE:
                    return Vector2I.Down + Vector2I.Right;
                default:
                    return Vector2I.Zero;
            }
        }
    }
}