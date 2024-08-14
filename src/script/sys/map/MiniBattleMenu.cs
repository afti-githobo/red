using Godot;
using Red.Battle;
using Red.MapScene;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Red.Sys.MapScene
{
    [GlobalClass]
    public partial class MiniBattleMenu : Control, IManagedMenu
    {
        [Export]
        public Color AttackerColor { get; private set; }

        [Export]
        public Color DefenderColor { get; private set; }

        private MiniBattleSubmenu Left;
        private MiniBattleSubmenu Right;

        private MiniBattleSubmenu Attacker;
        private MiniBattleSubmenu Defender;

        public override void _Ready()
        {
            Left = GetNode<MiniBattleSubmenu>("Left");
            Right = GetNode<MiniBattleSubmenu>("Right");
            BattleRunner.OnCommit += BattleRunner_OnCommit;
        }

        private void BattleRunner_OnCommit(object sender, BattleRunner.OnCommitEventArgs e)
        {
            Attacker.Configure(isAttacker: true);
            Defender.Configure(isAttacker: false);
        }

        public override void _ExitTree()
        {
            BattleRunner.OnCommit -= BattleRunner_OnCommit;
        }

        public Task Close()
        {
            CallDeferred(CanvasItem.MethodName.SetVisible, false);
            return Task.CompletedTask;
        }

        public Task Open(params string[] args)
        {
            Trace.Assert(BattleRunner.CurrentBattle.Attacker.UnitRef.TryGetTarget(out var attacker), "Attacker UnitRef must not be null");
            Trace.Assert(BattleRunner.CurrentBattle.Attacker.UnitRef.TryGetTarget(out var defender), "Defender UnitRef must not be null");
            if (attacker.Orientation == Cardinal.E)
            {
                Attacker = Left;
                Defender = Right;
            }
            else
            {
                Defender = Left;
                Attacker = Right;
            }
            Attacker.Configure(isAttacker: true);
            Defender.Configure(isAttacker: false);
            CallDeferred(CanvasItem.MethodName.SetVisible, true);
            return Task.CompletedTask;
        }

        public Task ReclaimFocusFrom(IManagedMenu menu)
        {
            return Task.CompletedTask;
        }

        public Task SurrenderFocusTo(IManagedMenu menu)
        {
            return Task.CompletedTask;
        }
    }
}