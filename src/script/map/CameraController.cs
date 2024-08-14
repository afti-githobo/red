using Godot;
using Red.Data;

namespace Red.MapScene
{
    [GlobalClass]
    public partial class CameraController : Node
    {
        [Export]
        private Camera2D camera;
        [Export]
        private Vector2I tileFocus;
        [Export]
        private double baseMovementDelay;
        [Export]
        private double angularDelayCorrection;

        private double remainingMovementDelay;
        private Vector2I prevTileFocus;

        public override void _Ready()
        {
            Singleton.InstanceOf<TileCursor>().ChangedSelectedTile += _ChangedSelectedTile;  
        }

        public override void _Process(double delta)
        {
            if (remainingMovementDelay > 0)
            {
                remainingMovementDelay -= delta;
                if (remainingMovementDelay <= 0)
                {
                    // always do this as exactly as possible if done moving
                    camera.Offset = new Vector2(tileFocus.X * ProjectConstants.RealSizeOfOneTile, tileFocus.Y * ProjectConstants.RealSizeOfOneTile);
                }
                else
                {
                    Vector2 impulse = tileFocus - prevTileFocus;
                    impulse *= ProjectConstants.RealSizeOfOneTile;
                    double x = Mathf.Lerp(impulse.X, 0, remainingMovementDelay / baseMovementDelay);
                    double y = Mathf.Lerp(impulse.Y, 0, remainingMovementDelay / baseMovementDelay);
                    // 1px = 1 unit, so!
                    x = Mathf.Round(x);
                    y = Mathf.Round(y);
                    x += prevTileFocus.X * ProjectConstants.RealSizeOfOneTile;
                    y += prevTileFocus.Y * ProjectConstants.RealSizeOfOneTile;
                    camera.Offset = new Vector2((float)x, (float)y);
                }
            }
        }

        public bool StillMoving() => remainingMovementDelay > 0;

        public void SetTileFocus(Vector2I coords, double movementTime = -1)
        {
            if (movementTime < 0) movementTime = GetMovementTime(tileFocus, coords);
            prevTileFocus = tileFocus;
            tileFocus = coords;
            remainingMovementDelay = movementTime;
            if (remainingMovementDelay == 0)
            {
                camera.Offset = new Vector2(tileFocus.X * ProjectConstants.RealSizeOfOneTile, tileFocus.Y * ProjectConstants.RealSizeOfOneTile); 
            }
        }

        public double GetMovementTime(Vector2I from, Vector2I to)
        {
            Vector2I impulse = (to - from);
            float len = Mathf.Abs(impulse.Length());
            // between 0 and 1, based on how far the vector is from the X/Y axes - 0 at an axis, 1 at diagonal
            float adj;
            if (len == 0) adj = 0;
            else if (Mathf.Abs(impulse.X) > Mathf.Abs(impulse.Y)) adj = (float)impulse.Y / impulse.X;
            else adj = (float)impulse.X / impulse.Y;
            double delayPerTile = baseMovementDelay;
            delayPerTile *= Mathf.Lerp(1, angularDelayCorrection, Mathf.Abs(adj));
            return delayPerTile * len;
        }

        private void _ChangedSelectedTile(Vector2I coords, int _, int __) => SetTileFocus(coords, baseMovementDelay);
    }
}