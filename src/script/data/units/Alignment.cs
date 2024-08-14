using Red.MapScene;
using System;

namespace Red.Data.Units
{
    public enum Alignment
    {
        None = 0,
        Player = 1 << 1,
        Enemy = 1 << 2,
        Ally = 1 << 3,
        Neutral = 1 << 4
    }

    public static partial class Extensions
    {
        public static TurnPhase PrimaryPhase (this Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Player:
                    return TurnPhase.Player;
                case Alignment.Enemy:
                    return TurnPhase.Enemy;
                case Alignment.Ally:
                    return TurnPhase.Ally;
                case Alignment.Neutral:
                    return TurnPhase.Neutral;
                default:
                    throw new Exception($"Bad Alignment: {alignment}");
            }
        }
    }
}