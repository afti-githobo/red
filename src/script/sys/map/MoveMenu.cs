using Godot;
using Red.Data.Items;
using Red.Data.Units;
using Red.MapScene;
using Red.MapScene.Units;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Red.Sys.MapScene
{
    [GlobalClass]
    public partial class MoveMenu : Control, IManagedMenu
    {
        public enum Mode
        {
            None = 0,
            Move = 1,
            Target = 2,
            Length = 3
        }

        public static MoveMenu Instance { get; private set; }

        [Export]
        private Vector2I topLeft;
        [Export]
        private Vector2I bottomRight;
        [Export]
        private Vector2I tileUnreachable;
        [Export]
        private Vector2I tileReachable;
        [Export]
        private Vector2I tileOccupied;

        [Export]
        private double minTimeOpenBeforeClose;

        private bool isOpen;

        private Unit unit;
        private Unit.PathfindingStruct paths;

        [Export]
        double baseInputDelay;

        [Export]
        double inputDelay;

        private Mode mode;

        public override void _Ready()
        {
            Trace.Assert(Instance == null, "There should only be one instance of TileGuide!");
            Instance = this;
            Visible = false;
            ISingleton<InputHandler>.Instance.ConfirmMenuOption += ConfirmMenuOption;
        }

        public override void _Process(double delta)
        {
            if (inputDelay > 0) inputDelay -= delta;
        }

        private void ConfirmMenuOption()
        {
            GD.Print(inputDelay);
            if (inputDelay > 0 || !isOpen) return;
            var controller = unit.Controller as PlayerUnitController;
            switch (mode)
            {
                case Mode.Move:
                    var p = GetPathToCoords();
                    if (p != null) controller.SetMove(p);
                    else if (AreCurrentCoordsTargetable())
                    {
                        // nop - will need to add attacks
                    }
                    break;
                case Mode.Target:
                    var t = Singleton.InstanceOf<UnitManager>().GetUnitAtPosition(Singleton.InstanceOf<TileCursor>().GetPosition());
                    var w = unit.Data.GetCurrentWeapon();
                    // start battle...
                    break;
            }

            ISingleton<MenuSystem>.Instance.CloseMenu<MoveMenu>();
        }

        private Vector2I[] GetPathToCoords()
        {
            var coords = Singleton.InstanceOf<TileCursor>().GetPosition();
            var path = paths.navigablePaths.First;
            while (path != null)
            {
                if (path.Value[path.Value.Length - 1] == coords) return path.Value;
                path = path.Next;
            }
            return null;
        }

        bool AreCurrentCoordsTargetable()
        {
            var coords = Singleton.InstanceOf<TileCursor>().GetPosition();
            return paths.attackableTiles.Contains(coords);
        }

        public override void _ExitTree()
        {
            Instance = null;
        }

        public async void ShowFor (Unit unit, Item wpn = null, bool isFriendlyInteraction = false, TargetFlags flags = TargetFlags.None, Mode m = Mode.Move)
        {
            mode = m;
            Visible = true;
            this.unit = unit;
            while (!unit.IsPathfindingResultCurrent)
            {
                GD.Print(".");
                await Task.Delay(100);
            }
            switch (mode)
            {
                case Mode.Move:
                    paths = unit.PathfindingResult;
                    var path = paths.navigablePaths.First;
                    foreach (var tile in paths.attackableTiles)
                    {
                        Singleton.InstanceOf<MovePreviewLayer>().SetCell(-1, tile, sourceId: 0, atlasCoords: tileUnreachable);
                    }
                    while (path != null)
                    {
                        for (int i = 0; i < path.Value.Length; i++)
                        {
                            Singleton.InstanceOf<MovePreviewLayer>().SetCell(-1, path.Value[i], sourceId: 0, atlasCoords: tileReachable);
                        }
                        path = path.Next;
                    }
                    Singleton.InstanceOf<MovePreviewLayer>().SetCell(-1, unit.Data.LogicalPosition, sourceId: 0, atlasCoords: tileOccupied);
                    break;
                case Mode.Target:
                    Trace.Assert(flags != TargetFlags.None, "Shouldn't be opening MoveMenu for targeting w/o specifying target flags");
                    var targetingInfo = unit.GetTargetsForItem(wpn);
                    for (int i = 0; i < targetingInfo.Item1.Count; i++)
                    {
                        if (flags == TargetFlags.Enemy) Singleton.InstanceOf<MovePreviewLayer>().SetCell(-1, targetingInfo.Item1[i].Data.LogicalPosition, sourceId: 0, atlasCoords: tileUnreachable);
                        else Singleton.InstanceOf<MovePreviewLayer>().SetCell(-1, targetingInfo.Item1[i].Data.LogicalPosition, sourceId: 0, atlasCoords: tileOccupied);
                    }
                    for (int i = 0; i < targetingInfo.Item2.Count; i++)
                    {
                        Singleton.InstanceOf<MovePreviewLayer>().SetCell(-1, targetingInfo.Item2[i], sourceId: 0, atlasCoords: tileReachable);
                    }
                    break;
                default:
                    GD.PrintErr($"Shouldn't be opening MoveMenu with invalid mode {m}");
                    break;
            }

        }

        public Task Close()
        {
            CallDeferred(CanvasItem.MethodName.SetVisible, false);
            Singleton.InstanceOf<MovePreviewLayer>().Clear();
            isOpen = false;
            return Task.CompletedTask;
        }

        public Task Open(params string[] args)
        {
            var mode = Mode.Move;
            if (args.Length > 0) mode = Enum.Parse<Mode>(args[0]);
            inputDelay = baseInputDelay;
            GD.Print($"o {inputDelay}");
            var unit = Singleton.InstanceOf<UnitManager>().GetUnitAtPosition(Singleton.InstanceOf<TileCursor>().GetPosition());
            switch(mode)
            {
                case Mode.Move:
                    ShowFor(unit);
                    break;
                case Mode.Target:
                    ShowFor(unit, unit.Data.GetCurrentWeapon(), false, ItemSpec.Of(unit.Data.GetCurrentWeapon().ItemID).TargetFlags, mode);
                    break;
            }      
            isOpen = true;
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