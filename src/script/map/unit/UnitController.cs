using Godot;
using Red.Battle;
using Red.Data.Items;
using System;
using System.Threading.Tasks;

namespace Red.MapScene.Units
{
    public abstract partial class UnitController : Node
    {
        protected Unit unit { get; private set; }

        public readonly struct ActionStruct
        {
            public readonly Vector2I[] MovementPath;
            public readonly bool EndTurn;
            public readonly Item Item;
            public readonly Unit Target;

            public ActionStruct(Vector2I[] movementPath, bool endTurn, Item item, Unit target)
            {
                MovementPath = movementPath;
                EndTurn = endTurn;
                Item = item;
                Target = target;
            }
        }

        public override void _Ready()
        {
            unit = GetParent<Unit>();
            if (unit == null) throw new Exception("UnitController must be a child of Unit!");
            unit.OnMainPhase += OnMainPhase;
        }

        private async void OnMainPhase(object _, UnitEventArgs e)
        {
            if (e.Unit.Exists && e.Unit.Actionable)
            {
                ((ILockable)ISingleton<TurnManager>.Instance).LockAs(unit);
                while (e.Unit.Actionable)
                {
                    var action = await GetAction(e.TurnCount);
                    GD.Print("got action");
                    ((ILockable)ISingleton<InputHandler>.Instance).LockAs(unit);
                    if ((action.MovementPath?.Length ?? -1) > 0)
                    {
                        await unit.Move(action.MovementPath);
                    }
                    if (action.EndTurn) unit.SetActionable(false);
                    if (action.Item != null)
                    {
                        if ((ItemSpec.Of(action.Item.ItemID).ItemFlags & ItemFlags.Weapon) != ItemFlags.None)
                        {
                            BattleRunner.Start(new BattleData(e.Unit.Data, action.Target.Data, action.Item, action.Target.Data.GetCurrentWeapon(), 999));
                        }
                    }
                    ((ILockable)ISingleton<InputHandler>.Instance).UnlockAs(unit);
                }
                ((ILockable)ISingleton<TurnManager>.Instance).UnlockAs(unit);
            }
        }

        public override void _ExitTree()
        {
            unit.OnMainPhase -= OnMainPhase;
        }

        public abstract Task<ActionStruct> GetAction(int turnCount);
    }
}