using Red.Data.Items;
using Red.Data.Units;

namespace Red.Battle
{
    public readonly struct BattleData
    {
        public readonly UnitData Attacker;
        public readonly UnitData Defender;
        public readonly Item AttackerWeapon;
        public readonly Item DefenderWeapon;
        public readonly ulong RNGSeed;

        public BattleData(UnitData attacker, UnitData defender, Item attackerWeapon, Item defenderWeapon,ulong rngSeed)
        {
            Attacker = attacker;
            Defender = defender;
            AttackerWeapon = attackerWeapon;
            DefenderWeapon = defenderWeapon;
            RNGSeed = rngSeed;
        }
    }
}