using Godot;
using Red.Data;
using Red.Data.Items;
using Red.Data.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Red.Battle
{
    public static class BattleRunner
    {
        public class OnCommitEventArgs : EventArgs
        {
            public readonly (BattleStep, int) Data;

            public OnCommitEventArgs((BattleStep, int) data)
            {
                Data = data;
            }
        }

        private static RandomNumberGenerator rng = new RandomNumberGenerator();
        private static Queue<(BattleStep, int)> StepQueue = new();

        public static event EventHandler<OnCommitEventArgs> OnCommit;

        public static BattleData CurrentBattle { get; private set; }

        public static void Start(BattleData battle)
        {
            rng.Seed = battle.RNGSeed;
            Trace.Assert(StepQueue.Count == 0, "Shouldn't be a battle in progress");
            CurrentBattle = battle;
            (BattleStep, int) hit = (BattleStep.AttackerAttack, 0);
            // initial hit
            HandleBattleStep(ref hit, CurrentBattle.Attacker, CurrentBattle.Defender, CurrentBattle.AttackerWeapon, CurrentBattle.DefenderWeapon);
            StepQueue.Enqueue(hit);
            // if still alive and armed, counterhit
            if (!(hit.Item1.HasFlag(BattleStep.FatalDamage)) && CurrentBattle.DefenderWeapon != null && CurrentBattle.Defender.CanWieldWeapon(CurrentBattle.DefenderWeapon))
            {
                hit.Item1 = BattleStep.DefenderAttack;
                HandleBattleStep(ref hit, CurrentBattle.Defender, CurrentBattle.Attacker, CurrentBattle.DefenderWeapon, CurrentBattle.AttackerWeapon);
                StepQueue.Enqueue(hit);
            }
            // if still alive, able to double, and weapon is not broken on first hit...
            if (!(hit.Item1.HasFlag(BattleStep.FatalDamage)) && 
                (CurrentBattle.Attacker.GetAdjustedSpeed(CurrentBattle.AttackerWeapon) / CurrentBattle.Defender.GetAdjustedSpeed(CurrentBattle.DefenderWeapon)) > ProjectConstants.DoubleAttackRatio &&
                CurrentBattle.AttackerWeapon.Durability > 1)
            {
                hit.Item1 = BattleStep.AttackerAttack;
                HandleBattleStep(ref hit, CurrentBattle.Attacker, CurrentBattle.Defender, CurrentBattle.AttackerWeapon, CurrentBattle.DefenderWeapon);
                StepQueue.Enqueue(hit);
            }

            static void HandleBattleStep(ref (BattleStep, int) hit, UnitData attacker, UnitData defender, Item attackerWpn, Item defenderWpn)
            {
                hit.Item2 = attacker.GetAttackDamage(attackerWpn, defenderWpn, defender);
                var adjustedEvasion = Mathf.Clamp(defender.GetEvasion(defenderWpn) - attacker.GetAccuracy(attackerWpn), 0, 100);
                var adjustedCrit = Mathf.Clamp(attacker.GetCritRate(attackerWpn) - defender.GetCritEvade(defenderWpn), 1, 100);
                if (rng.RandfRange(0, 100) < adjustedEvasion)
                {
                    hit.Item1 |= BattleStep.Miss;
                    hit.Item2 = 0;
                }
                else if (rng.RandfRange(0, 100) < adjustedCrit)
                {
                    hit.Item1 |= BattleStep.Critical;
                    hit.Item2 = attacker.GetAttackDamage(attackerWpn, defenderWpn, defender, isCritical: true);
                }
                if (hit.Item2 == defender.CurrentHP) hit.Item1 |= BattleStep.FatalDamage;
            }
        }

        public static bool Step(out (BattleStep, int) v)
        {
            if (StepQueue.Count > 0)
            {
                v = StepQueue.Dequeue();
                return true;
            }
            v = default;
            return false;
        }

        public static void Commit((BattleStep, int) step)
        {
            OnCommit?.Invoke(null, new OnCommitEventArgs(step));
            if (step.Item1.HasFlag(BattleStep.AttackerAttack))
            {
                CurrentBattle.Defender.SetCurrentHP(CurrentBattle.Defender.CurrentHP - step.Item2);
            }
        }
    }
}