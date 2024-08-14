using Godot;

namespace Red.MapScene
{
    [GlobalClass]
    public partial class MovePreviewLayer : TileMap, ISingleton<MovePreviewLayer>
    {
        public override void _Ready()
        {
            ((ISingleton<MovePreviewLayer>)this).__Ready();
        }

        public override void _ExitTree()
        {
            ((ISingleton<MovePreviewLayer>)this).__ExitTree();
        }
    }
}