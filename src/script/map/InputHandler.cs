using Godot;
using Red.Sys.MapScene;
using System;
using System.Collections.Generic;

namespace Red.MapScene
{
    [GlobalClass]
    public partial class InputHandler : Node, ISingleton<InputHandler>, ILockable
    {

        [Signal]
        public delegate void ConfirmMenuOptionEventHandler();

        [Signal]
        public delegate void ChangeContextMenuOptionsEventHandler(int options);

        [Signal]
        public delegate void CloseContextMenuEventHandler();

        [Signal]
        public delegate void OpenContextMenuEventHandler();

        [Signal]
        public delegate void SelectContextMenuOptionEventHandler(int index);

        [Signal]
        public delegate void UIDirectionalInputEventHandler(Vector2I dir);

        [Export]
        private string inputOpenContextMenu;
        [Export]
        private string inputCloseContextMenu;
        [Export]
        private string inputSelectContextMenuOption;
        [Export]
        private string inputLeft;
        [Export]
        private string inputRight;
        [Export]
        private string inputUp;
        [Export]
        private string inputDown;
        [Export]
        private bool acceptingInput;

        private LinkedList<ILockable.LockStruct> _locks = new LinkedList<ILockable.LockStruct>();
        public LinkedList<ILockable.LockStruct> Locks { get => _locks; }

        public override void _Ready()
        {
            ((ISingleton<InputHandler>)this).__Ready();
        }

        public override void _ExitTree()
        {
            ((ISingleton<InputHandler>)this).__ExitTree();
        }

        public override void _Process(double delta)
        {
            if (acceptingInput && Locks.Count == 0)
            {
                if (Input.IsActionPressed(inputLeft))
                {
                    if (Input.IsActionPressed(inputUp)) EmitSignal(SignalName.UIDirectionalInput, Vector2I.Left + Vector2I.Up);
                    else if (Input.IsActionPressed(inputDown)) EmitSignal(SignalName.UIDirectionalInput, Vector2I.Left + Vector2I.Down);
                    else EmitSignal(SignalName.UIDirectionalInput, Vector2I.Left);
                }
                else if (Input.IsActionPressed(inputRight))
                {
                    if (Input.IsActionPressed(inputUp)) EmitSignal(SignalName.UIDirectionalInput, Vector2I.Right + Vector2I.Up);
                    else if (Input.IsActionPressed(inputDown)) EmitSignal(SignalName.UIDirectionalInput, Vector2I.Right + Vector2I.Down);
                    else EmitSignal(SignalName.UIDirectionalInput, Vector2I.Right);
                }
                else if (Input.IsActionPressed(inputUp)) EmitSignal(SignalName.UIDirectionalInput, Vector2I.Up);
                else if (Input.IsActionPressed(inputDown)) EmitSignal(SignalName.UIDirectionalInput, Vector2I.Down);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (acceptingInput && Locks.Count == 0)
            {
                if (@event.IsActionReleased(inputOpenContextMenu))
                {
                    EmitSignal(SignalName.OpenContextMenu);
                }
                if (@event.IsActionReleased(inputSelectContextMenuOption))
                {
                    EmitSignal(SignalName.ConfirmMenuOption);
                }
            }
        }
    }
}