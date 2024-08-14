using System.Linq;

namespace Red.Data.Units
{
    public enum Faction
    {
        None = 0,
        PlayerFaction = 1,
        MiscRed = 2,
        MiscGreen = 3,
        MiscYellow = 4,
    }

    public static partial class Extensions
    {
        // Eventually we'll have to tear the hardcoded tables here out and do some dynamic stuff, but not for a while yet!
        private static readonly Faction[] playerAllies = { Faction.MiscGreen, };
        private static readonly Faction[] playerEnemies = { Faction.MiscRed, };

        public static Alignment GetAlignment(this Faction f)
        {
            if (f.IsPlayerFaction()) return Alignment.Player;
            else if (f.IsPlayerEnemy()) return Alignment.Enemy;
            else if (f.IsPlayerAlly()) return Alignment.Ally;
            else return Alignment.Neutral;
        }

        private static bool IsPlayerAlly(this Faction f) => playerAllies.Contains(f);
        private static bool IsPlayerEnemy(this Faction f) => playerEnemies.Contains(f);

        private static bool IsPlayerFaction(this Faction f) => f == Faction.PlayerFaction;
    }
}