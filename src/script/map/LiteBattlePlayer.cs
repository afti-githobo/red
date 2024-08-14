using Red.Battle;
using Red.MapScene.Units;
using Red.Sys;
using Red.Sys.MapScene;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Red.MapScene
{
    public partial class LiteBattlePlayer : BattlePlayer
    {
        private Unit attacker;
        private Unit defender;

        protected override Task Setup()
        {
            Trace.Assert(BattleRunner.CurrentBattle.Attacker.UnitRef.TryGetTarget(out attacker), "Shouldn't be trying to do a battle when attacker/defender don't have their UnitRefs");
            Trace.Assert(BattleRunner.CurrentBattle.Attacker.UnitRef.TryGetTarget(out defender), "Shouldn't be trying to do a battle when attacker/defender don't have their UnitRefs");
            return Singleton.InstanceOf<MenuSystem>().OpenMenu<MiniBattleMenu>();
        }

        protected override Task PreBattle()
        {
            attacker.Fight(defender);
            return Task.CompletedTask;
        }

        protected override Task HandleBattleStep((BattleStep, int) step)
        {
            BattleRunner.Commit(step);
            if (step.Item1.HasFlag(BattleStep.AttackerAttack)) return attacker.FieldAttack(() => { BattleRunner.Commit(step); } );
            else if (step.Item1.HasFlag(BattleStep.DefenderAttack)) return defender.FieldAttack(() => { BattleRunner.Commit(step); });
            return Task.CompletedTask;
        }

        protected override Task PostBattle()
        {
            if (BattleRunner.CurrentBattle.Attacker.Dead)
            {

            }
            if (BattleRunner.CurrentBattle.Defender.Dead)
            {

            }
            return Task.CompletedTask;
        }

        protected override Task Cleanup()
        {
            return Task.CompletedTask;
        }
    }
}