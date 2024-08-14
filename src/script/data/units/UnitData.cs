using Godot;
using Godot.Collections;
using Red.Data.Classes;
using Red.Data.Items;
using Red.MapScene.Units;
using System;

namespace Red.Data.Units
{
    [GlobalClass]
    public partial class UnitData : Resource
    {
        public const int MinHP = 0;
        public const int HPFloor = 1;
        public const int HPCap = 99;
        public const int StatFloor = 1;
        public const int StatCap = 99;
        public const int LevelFloor = 1;
        public const int LevelCap = 20;
        public const int MoveFloor = 0;
        public const int MoveCap = 20;
        public const int HungerFloor = 0;
        public const int HungerCap = 100;
        public const int CurseFloor = 0;
        public const int CurseCap = 99;
        public const int MoneyFloor = 0;
        public const int MoneyCap = 20000;
        public const int FoodFloor = 0;
        public const int FoodCap = 30;

        public const float CritDeteriminant = 1;
        public const float HitDeteriminant = 0.25f;

        public WeakReference<Unit> UnitRef;

        [Export]
        public string Name { get; private set; }
        [Export]
        public int Level { get; private set; }
        [Export]
        public int Exp { get; private set; }
        [Export]
        public ClassID Class { get; private set; }
        [Export]
        public Faction Faction { get; private set; }
        [Export]
        public int Move { get; private set; }
        [Export]
        public int MaxHP { get; private set; }
        [Export]
        public int CurrentHP { get; private set; }
        [Export]
        public int Strength { get; private set; }
        [Export]
        public int Defense { get; private set; }
        [Export]
        public int Dexterity { get; private set; }
        [Export]
        public int Speed { get; private set; }
        [Export]
        public int Luck { get; private set; }
        [Export]
        public int Constitution { get; private set; }
        [Export]
        public int Hunger { get; private set; }
        [Export]
        public int Curse { get; private set; }
        [Export]
        public int Money { get; private set; }
        [Export]
        public int Food { get; private set; }
        [Export]
        public bool Dead { get; private set; }
        [Export]
        public Vector2I LogicalPosition { get; private set; }
        [Export]
        public Array<Item> Inventory { get; private set; }

        const int InventorySize = 5;

        public void SetCurrentHP(int hp)
        {            
            if (hp < HPFloor) CurrentHP = HPFloor;
            else if (hp > MaxHP) CurrentHP = MaxHP;
            else CurrentHP = hp;
            if (CurrentHP == 0)
            {
                Dead = true;
            }
        }

        public void SetMaxHP(int hp)
        {        
            if (hp < HPFloor) MaxHP = HPFloor;
            else if (hp > HPCap) MaxHP = HPCap;
            else MaxHP = hp;
        }

        public void SetLogicalPosition(Vector2I pos)
        {
            LogicalPosition = pos;
        }

        public Item GetCurrentWeapon ()
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                if (Inventory[i] != null && (ItemSpec.Of(Inventory[i].ItemID).ItemFlags & ItemFlags.Weapon & ClassSpec.Of(Class).ItemFlags) != ItemFlags.None)
                    return Inventory[i];
            }
            return null;
        }

        public int GetAdjustedStrength(Item weapon = null) => Strength;
        public int GetAdjustedDefense(Item weapon = null) => Defense;
        public int GetAdjustedDexterity(Item weapon = null) => Mathf.FloorToInt(GetWeightModifier(weapon) * Dexterity);
        public int GetAdjustedSpeed(Item weapon = null) => Mathf.FloorToInt(GetWeightModifier(weapon) * Speed);
        public int GetAdjustedLuck(Item weapon = null) => Luck;

        public int GetAdjustedConstitution() => Constitution;

        private float GetWeightModifier(Item weapon)
        {
            if (weapon != null && ItemSpec.Of(weapon.ItemID).Weight > GetAdjustedConstitution())
            {
                return 1 / ((1 + (ItemSpec.Of(weapon.ItemID).Weight / GetAdjustedConstitution())) / 2);
            }
            return 1;
        }

        public float GetAccuracy(Item weapon)
        {
            return Mathf.Clamp(GetAdjustedDexterity(weapon) / HitDeteriminant, 0, 99);
        }

        public float GetEvasion(Item weapon)
        {
            return Mathf.Clamp(GetAdjustedSpeed(weapon) / HitDeteriminant, 0, 99);
        }

        public float GetCritRate(Item weapon)
        {
            return Mathf.Clamp((GetAdjustedLuck(weapon) + GetAdjustedSpeed(weapon)) / CritDeteriminant, 1, 50);
        }

        public float GetCritEvade(Item weapon)
        {
            return Mathf.Clamp(((GetAdjustedLuck(weapon) + GetAdjustedSpeed(weapon)) / CritDeteriminant) / 2, 1, 50);
        }

        public int GetAttackDamage(Item weapon, Item enemyWeapon, UnitData enemy, bool isCritical = false)
        {
            var critMulti = isCritical ? ProjectConstants.CriticalMulti : 1;
            return Mathf.Clamp((ItemSpec.Of(weapon.ItemID).Power + GetAdjustedStrength(weapon) * critMulti) - enemy.GetAdjustedDefense(enemyWeapon), 0, enemy.CurrentHP);
        }

        public bool CanWieldWeapon(Item weapon)
        {
            return (ClassSpec.Of(Class).ItemFlags & ItemSpec.Of(weapon.ItemID).ItemFlags) != 0 && // is usable?
                (ItemSpec.Of(weapon.ItemID).ItemFlags & ItemFlags.Weapon) != 0 && // is weapon?
                weapon.Durability > 0; // is unbroken?
        }
    }
}