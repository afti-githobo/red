using System;

namespace Red.Data.Units
{
    [Flags]
    public enum TargetFlags
    {
        None = 0,
        Self = 1,
        Player = 1 << 1,
        Enemy = 1 << 2,
        Neutral = 1 << 3,
        Ally = 1 << 4
    }
}