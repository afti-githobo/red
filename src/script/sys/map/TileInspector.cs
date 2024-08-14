using Godot;
using Red.Data.Map;
using Red.MapScene;
using System;
using System.Threading.Tasks;

namespace Red.Sys.MapScene
{
    [GlobalClass]
    public partial class TileInspector : Control
    {
        private Sprite2D terrainIcon;
        private RichTextLabel terrainLabel;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            terrainIcon = GetNode<Sprite2D>("TerrainIcon");
            terrainLabel = GetNode<RichTextLabel>("TerrainLabel");
            var cursor = GetNode<TileCursor>("../../../TileCursor");
            cursor.ChangedSelectedTile += TileInspector_ChangedSelectedTile;
            var attrs = cursor.GetAttrsAtPosition();
            TileInspector_ChangedSelectedTile(cursor.GetPosition(), (int)attrs.Item1, (int)attrs.Item2);
        }

        private void TileInspector_ChangedSelectedTile(Vector2I pos,int attr, int subAttr)
        {
            terrainLabel.Text = GetTerrainLabelString((TileAttribute)attr);
        }

        private string GetTerrainLabelString(TileAttribute attr)
        {   
            // TODO: This should be an actual table! Right now, it's... not.
            return attr.ToString();
        }
    }

}