using System;

namespace Red.MapScene
{
    [Flags]
    public enum TurnPhase
    {
        None     = 0,
        // Sub-phases
        PreMain  = 0x0001 << 0,
        Main     = 0x0001 << 1,
        PostMain = 0x0001 << 2,
        AllSubPhases = PreMain | Main | PostMain,
        // Primary phases
        Player   = 0xF000 << 0,
        Ally     = 0xF000 << 1,
        Neutral  = 0xF000 << 2,
        Enemy    = 0xF000 << 3,
        AllPrimaryPhases = Player | Ally | Neutral | Enemy,
    }

    public static partial class Extensions
    {
        public static TurnPhase PrimaryPhase(this TurnPhase turnPhase) => turnPhase & TurnPhase.AllPrimaryPhases;
        public static TurnPhase SubPhase(this TurnPhase turnPhase) => turnPhase & TurnPhase.AllSubPhases;
    }
}