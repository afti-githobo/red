using Godot;
using Red.MapScene.Units;
using System.Diagnostics;

namespace Red.Sys.MapScene
{
    [GlobalClass]
    public partial class UnitHPMiniBar : Panel
    {
        Unit parent;
        float defaultWidth;

        [Export]
        private Color colorHealthy;
        [Export]
        private Color colorDamaged;
        [Export]
        private float damagedThreshold;
        [Export]
        private Color colorCritical;
        [Export]
        private float criticalThreshold;

        public override void _Ready()
        {
            parent = GetParent<Unit>();
            Trace.Assert(parent != null, "UnitHPMiniBar must be a child of Unit");
            parent.HPChanged += HPChanged;
            defaultWidth = Size.X;
        }

        private void HPChanged(int prevHP, int curHP)
        {
            var percentage = curHP / parent.Data.MaxHP;
            Size = new Vector2(defaultWidth * percentage, Size.Y);
            if (percentage <= criticalThreshold) AddThemeColorOverride(ThemeTypeVariation, colorCritical);
            else if (percentage <= damagedThreshold) AddThemeColorOverride(ThemeTypeVariation, colorDamaged);
            else AddThemeColorOverride(ThemeTypeVariation, colorHealthy);
        }
    }
}
