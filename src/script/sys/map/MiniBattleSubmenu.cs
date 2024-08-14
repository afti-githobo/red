using Godot;
using Red.Battle;
using Red.Data.Items;
using Red.Data.Units;

namespace Red.Sys.MapScene
{
    public partial class MiniBattleSubmenu : Control
    {
        private MiniBattleMenu parent;

        private RichTextLabel nameLabel;
        private RichTextLabel weaponLabel;
        private RichTextLabel hpLabel;
        private Panel panel;
        private Panel hpBar;

        private float baseHpBarWidth;

        public override void _Ready()
        {
            parent = GetParent<MiniBattleMenu>();
            nameLabel = GetNode<RichTextLabel>("NameLabel");
            hpLabel = GetNode<RichTextLabel>("HPLabel");
            panel = GetNode<Panel>("Panel");
            hpBar = GetNode<Panel>("HPBar");
            baseHpBarWidth = hpBar.Size.X;
        }

        public void Configure(bool isAttacker = false)
        {
            if (isAttacker) _Configure(parent.AttackerColor, BattleRunner.CurrentBattle.Attacker, BattleRunner.CurrentBattle.AttackerWeapon);
            else _Configure(parent.DefenderColor, BattleRunner.CurrentBattle.Defender, BattleRunner.CurrentBattle.DefenderWeapon);

            void _Configure(Color color, UnitData unit, Item weapon)
            {
                panel.AddThemeColorOverride(ThemeTypeVariation, color);
                nameLabel.Text = unit.Name;
                weaponLabel.Text = ItemSpec.Of(weapon.ItemID).Name;
                hpLabel.Text = unit.CurrentHP.ToString();
                hpBar.Size = new Vector2(baseHpBarWidth * (unit.CurrentHP / unit.MaxHP), hpBar.Size.Y);
            }
        }
    }
}