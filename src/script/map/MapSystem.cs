using Godot;

namespace Red.MapScene
{
	/// <summary>
	/// Main gameplay controller class for the map scene.
	/// </summary>
	[GlobalClass]
	public partial class MapSystem : Node2D, ISingleton<MapSystem>
	{
		public Map CurrentMap { get; private set; }

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			((ISingleton<MapSystem>)this).__Ready();
        }

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (CurrentMap == null) CurrentMap = GetNode<Map>("CurMap");
		}

        public override void _ExitTree()
        {
            ((ISingleton<MapSystem>)this).__ExitTree();
        }
    }

}
