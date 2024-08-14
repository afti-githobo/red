using Godot;
using Red.Data;
using Red.Data.Map;
using Red.Sys.MapScene;

namespace Red.MapScene
{
    [GlobalClass]
    public partial class TileCursor : Node2D, ISingleton<TileCursor>
    {

		[Signal]
		public delegate void ChangedSelectedTileEventHandler(Vector2I pos, int attr, int subAttr);

		[Export]
		Vector2I logicalPosition;
		[Export]
		double movementCooldown;

		double timeSinceLastMovement;

        public override void _Ready()
        {
			((ISingleton<TileCursor>)this).__Ready();
			ISingleton<InputHandler>.Instance.UIDirectionalInput += _UIDirectionalInput;
            base._Ready();
        }

        public override void _ExitTree()
        {
			((ISingleton<TileCursor>)this).__ExitTree();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
		{
			if (timeSinceLastMovement < movementCooldown) timeSinceLastMovement += delta;
		}

        public bool AcceptingInput() => timeSinceLastMovement >= movementCooldown && !(ContextMenu.Instance?.IsOpen ?? true);

		public Vector2I GetPosition() => logicalPosition;

		public (TileAttribute, TileSubAttribute) GetAttrsAtPosition() => Singleton.InstanceOf<MapSystem>().CurrentMap.GetTileAttributes(logicalPosition.X, logicalPosition.Y);

        public bool SetPosition(Vector2I p, bool force = false)
        {
			if ((AcceptingInput() || force) && p != logicalPosition)
			{
				logicalPosition = p;
				Transform2D t = Transform2D.Identity.Translated(
					new Vector2(logicalPosition.X * ProjectConstants.RealSizeOfOneTile, logicalPosition.Y * ProjectConstants.RealSizeOfOneTile));
				Transform = t;
				timeSinceLastMovement = 0;
				var attrs = GetAttrsAtPosition();
                EmitSignal(SignalName.ChangedSelectedTile, logicalPosition, (int)attrs.Item1, (int)attrs.Item2);
				return true;
			}
			else return false;
		}

		private void _UIDirectionalInput(Vector2I dir)
		{
			if (AcceptingInput())
			{
                Vector2I pos = GetPosition();
                if (dir.X < 0) pos += Vector2I.Left;
                else if (dir.X > 0) pos += Vector2I.Right;
                if (dir.Y < 0) pos += Vector2I.Up;
                else if (dir.Y > 0) pos += Vector2I.Down;
                SetPosition(pos);
            }
		}

	}
}