using System;

namespace Red.Data.Classes
{
    [Flags]
    public enum ActionFlags
    {
        None = 0,
        Move = 1,
        Attack = 1 << 1,
        Camp = 1 << 2,
        Item = 1 << 3,
        Staff = 1 << 4
    }
}