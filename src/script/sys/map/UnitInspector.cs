using Godot;
using Red.Data.Classes;
using Red.Data.Items;
using Red.MapScene;
using Red.MapScene.Units;

namespace Red.Sys.MapScene
{
    [GlobalClass]
    public partial class UnitInspector : Control
    {
        private Sprite2D unitIcon;
        private RichTextLabel nameLabel;
        private RichTextLabel classLabel;
        private RichTextLabel hpLabel;
        private RichTextLabel weaponLabel;
        
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            unitIcon = GetNode<Sprite2D>("UnitIcon");
            nameLabel = GetNode<RichTextLabel>("NameLabel");
            classLabel = GetNode<RichTextLabel>("ClassLabel");
            hpLabel = GetNode<RichTextLabel>("HPLabel");
            weaponLabel = GetNode<RichTextLabel>("WeaponLabel");
            var cursor = Singleton.InstanceOf<TileCursor>();
            cursor.ChangedSelectedTile += UnitInspector_ChangedSelectedTile;
            var attrs = cursor.GetAttrsAtPosition();
            UnitInspector_ChangedSelectedTile(cursor.GetPosition(), (int)attrs.Item1, (int)attrs.Item2);
        }

        private void UnitInspector_ChangedSelectedTile(Vector2I pos,int attr, int subAttr)
        {
            var unit = Singleton.InstanceOf<UnitManager>()?.GetUnitAtPosition(pos);
            Visible = unit != null;
            if (unit != null)
            {
                nameLabel.Text = unit.Data.Name;
                classLabel.Text = ClassSpec.Of(unit.Data.Class).Name;
                hpLabel.Text = $"{unit.Data.CurrentHP}/{unit.Data.MaxHP}";
                var wpn = unit.Data.GetCurrentWeapon();
                if (wpn != null) weaponLabel.Text = ItemSpec.Of(wpn.ItemID).Name;
                else weaponLabel.Text = "-";
            }
        }
    }

}