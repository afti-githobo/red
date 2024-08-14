using System;

namespace Red.Battle
{
    [Flags]
    public enum BattleStep
    {
        None = 0,
        AttackerAttack = 1,
        DefenderAttack = 1 << 1,
        FatalDamage = 1 << 2,
        Critical = 1 << 3,
        Miss = 1 << 4
    }
}