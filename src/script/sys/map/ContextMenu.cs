using Godot;
using Red.Data.Units;
using Red.MapScene;
using Red.MapScene.Units;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Red.Sys.MapScene
{
    [GlobalClass]
    public partial class ContextMenu : Control, IManagedMenu
    {
        public static ContextMenu Instance { get; private set; }

        public bool IsOpen { get; private set; }

        [Flags]
        public enum Options
        {
            None = 0,
            Move = 1,
            Talk = 1 << 1,
            Use = 1 << 2,
            Open = 1 << 3,
            Visit = 1 << 4,
            Attack = 1 << 5,
            Done = 1 << 6,
            Status = 1 << 7,
            Quit = 1 << 8
        }

        private Options options = Options.Move | Options.Done | Options.Status | Options.Quit;
        private InputHandler inputHandler;
        private Node2D selectionPanel;
        private RichTextLabel label;
        private Panel bgPanel;

        private int selectedOptionIndex;
        private int currentOptionsCount = 3;

        [Export]
        int optionHeight;

        [Export]
        double baseInputDelay;

        [Export]
        double inputDelay;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            if (Instance != null) throw new Exception("Only one ContextMenu should exist!");
            Instance = this;
            Visible = false;
            ISingleton<InputHandler>.Instance.OpenContextMenu += _OpenContextMenuAsync;
            ISingleton<InputHandler>.Instance.UIDirectionalInput += _UIDirectionalInput;
            ISingleton<InputHandler>.Instance.ConfirmMenuOption += _ConfirmOption;
            selectionPanel = GetNode<Node2D>("SelectionPanel");
            bgPanel = GetNode<Panel>("BGPanel");
            label = GetNode<RichTextLabel>("Label");
        }

        public override void _Process(double delta)
        {
            if (inputDelay > 0) inputDelay -= delta;
        }

        private Options GetSelectedOption()
        {
            int indexBase = 0;
            int skipAdj = 0;

            while (indexBase <= selectedOptionIndex && 1 << (indexBase + skipAdj - 1) < (int)Options.Quit)
            {
                while (!options.HasFlag((Options)(1 << (indexBase + skipAdj)))) skipAdj++;
                indexBase++;
            }

            return (Options)(1 << (indexBase + skipAdj - 1));
        }

        private async void _ConfirmOption()
        {
            if (inputDelay > 0 || !IsOpen) return;
            var o = GetSelectedOption();
            GD.Print($"Selected: {o}");
            switch (o)
            {
                case Options.Move:
                    await ISingleton<MenuSystem>.Instance.CloseMenu<ContextMenu>();
                    await ISingleton<MenuSystem>.Instance.OpenMenu<MoveMenu>();          
                    break;
                case Options.Done:
                    var unit = Singleton.InstanceOf<UnitManager>().GetUnitAtPosition(Singleton.InstanceOf<TileCursor>().GetPosition());
                    var controller = unit.Controller as PlayerUnitController;
                    controller.SetDone();
                    await ISingleton<MenuSystem>.Instance.CloseMenu<ContextMenu>();
                    break;
                case Options.Attack:
                    await ISingleton<MenuSystem>.Instance.CloseMenu<ContextMenu>();
                    await ISingleton<MenuSystem>.Instance.OpenMenu<MoveMenu>("Target");
                    break;
                default:
                    await ISingleton<MenuSystem>.Instance.CloseMenu<ContextMenu>();
                    break;
            }
        }

        private void _ChangeOptions(Options o)
        {
            options = o;
            currentOptionsCount = 0;
            var b = new StringBuilder();
            for (int i = 1; i <= (int)Options.Quit; i<<=1)
            {
                if (o.HasFlag((Options)i))
                {
                    currentOptionsCount++;
                    b.AppendLine(((Options)i).ToString());
                }
            }
            label.Text = b.ToString();
            bgPanel.Size = new Vector2(bgPanel.Size.X, optionHeight * currentOptionsCount);
        }

        private async void _OpenContextMenuAsync()
        {
            var unit = Singleton.InstanceOf<UnitManager>().GetUnitAtPosition(Singleton.InstanceOf<TileCursor>().GetPosition());
            if (unit != null && !IsOpen) await ISingleton<MenuSystem>.Instance.OpenMenu<ContextMenu>();
        }

        private void _UIDirectionalInput(Vector2I dir)
        {
            if (inputDelay <= 0)
            {
                if (dir.Y < 0)
                {
                    selectedOptionIndex--;
                    if (selectedOptionIndex < 0) selectedOptionIndex = currentOptionsCount - 1;
                }
                else if (dir.Y > 0)
                {
                    selectedOptionIndex++;
                    if (selectedOptionIndex >= currentOptionsCount) selectedOptionIndex = 0;
                }
                inputDelay = baseInputDelay;
                _SelectOption(selectedOptionIndex);
            }
        }

        private void _SelectOption(int index)
        {
            Transform2D t = Transform2D.Identity.Translated(Vector2.Down * index * optionHeight);
            selectionPanel.Transform = t;
            selectedOptionIndex = index;
        }

        public override void _ExitTree()
        {
            Instance = null;
        }

        public Task Close()
        {
            IsOpen = false;
            CallDeferred(CanvasItem.MethodName.SetVisible, false);
            return Task.CompletedTask;
        }

        public Task Open(params string[] args)
        {
            IsOpen = true;
            selectionPanel.Transform = Transform2D.Identity;
            selectedOptionIndex = 0;
            var unit = Singleton.InstanceOf<UnitManager>().GetUnitAtPosition(Singleton.InstanceOf<TileCursor>().GetPosition());
            if (unit.Data.Faction.GetAlignment() == Alignment.Player)
            {
                var o = Options.Status | Options.Quit;
                if (unit.Actionable)
                {
                    o |= Options.Done;
                    var wpn = unit.Data.GetCurrentWeapon();
                    if (!unit.HasMoved) o |= Options.Move;
                    if (wpn != null && unit.GetTargetsForItem(wpn).Item1.Count > 0) o |= Options.Attack;
                }
                _ChangeOptions(o);
            }
            else _ChangeOptions(Options.Status | Options.Quit);
            CallDeferred(CanvasItem.MethodName.SetVisible, true);
            inputDelay = baseInputDelay;
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