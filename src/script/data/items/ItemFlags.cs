using System;

namespace Red.Data.Items
{
    [Flags]
    public enum ItemFlags
    {
        None = 0,
        Consumable = 1,
        Bow = 1 << 1,
        Sword = 1 << 2,
        Axe = 1 << 3,
        Lance = 1 << 4,
        Weapon = Bow | Sword | Axe | Lance
    }
}