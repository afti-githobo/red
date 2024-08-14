using Godot;
using Red.Data;
using Red.Data.Classes;
using Red.Data.Items;
using Red.Data.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Red.MapScene.Units
{
    [GlobalClass]
    public partial class Unit : AnimatedSprite2D
    {
        // This struct could be made zero-allocation if we declare the arrays as stackalloc and just use them as C arrays.
        // Would need to be unsafe to do that, though - so don't do it unless GC hits become a problem, but if they do...
        public readonly struct PathfindingStruct
        {
            public readonly LinkedList<Vector2I[]> navigablePaths;
            public readonly HashSet<Vector2I> attackableTiles;

            public PathfindingStruct(LinkedList<Vector2I[]> navigablePaths, HashSet<Vector2I> attackableTiles)
            {
                this.navigablePaths = navigablePaths;
                this.attackableTiles = attackableTiles;
            }
        }

        public enum AnimState
        {
            Neutral = 0,
            Moving = 1,
            Fighting = 2,
        }


        private AStarGridRed astar = new AStarGridRed();

        [Signal]
        public delegate void HPChangedEventHandler(int prevHP, int curHP);
        public event EventHandler<UnitEventArgs> OnPrimaryPhase;
        public event EventHandler<UnitEventArgs> OnPreMainPhase;
        public event EventHandler<UnitEventArgs> OnMainPhase;
        public event EventHandler<UnitEventArgs> OnPostMainPhase;


        [Export]
        public UnitData Data { get; private set; }
        [Export]
        public bool Actionable { get; private set; }

        [Export]
        private AnimState state = AnimState.Neutral;

        [Export]
        public Cardinal Orientation { get; private set; }

        private double timeInTransit;
        private Vector2I previousPosition;
        private Queue<Vector2I> pathQueue = new Queue<Vector2I>();
        private TaskCompletionSource movementTask;

        public bool HasMoved { get; private set; } = false;
        // there's nothing to do here rn but eventually we'll want to handle death, status, etc.
        public bool ShouldBeActionableAtPhaseStart => true;

        public UnitController Controller { get; private set; }

        /// <summary>
        /// Returns true if the unit exists (ie. is present in activeUnits and the scene tree) and false otherwise.
        /// 
        /// Used to enable lazy handling of unit caching - if a unit is returned to the pool while it's in a cache,
        /// it can just be skipped, instead of rebuilding the cache.
        /// </summary>
        public bool Exists => Data != null && Singleton.InstanceOf<UnitManager>().GetAllUnits().Contains(this);

        public PathfindingStruct PathfindingResult { get; private set; }
        public bool IsPathfindingResultCurrent { get; private set; }

        public override void _Ready()
        {
            if (!(GetParent() is UnitManager))
                throw new Exception("All Units must be children of UnitManager!");
        }

        public override void _ExitTree()
        {
            ISingleton<TurnManager>.Instance.ChangedPrimaryPhase -= ChangedPrimaryPhase;
            ISingleton<TurnManager>.Instance.PreMainPhase -= PreMainPhase;
            ISingleton<TurnManager>.Instance.PreMainPhase -= MainPhase;
            ISingleton<TurnManager>.Instance.PostMainPhase -= PostMainPhase;
            var controller = GetNode<UnitController>("Controller");
            controller?.QueueFree();
        }

        private void ChangedPrimaryPhase(int turnPhase, int turnCount)
        {
            if (Exists)
            {
                var phase = (TurnPhase)turnPhase;
                if (phase.PrimaryPhase() == Data.Faction.GetAlignment().PrimaryPhase())
                {
                    Actionable = ShouldBeActionableAtPhaseStart;
                }
            }
        }

        private void PreMainPhase(int turnPhase, int turnCount)
        {
            if (Exists)
            {
                GD.Print($"{Data.Name} PreMainPhase turn {turnCount},{((TurnPhase)turnPhase).PrimaryPhase()}");
                if (((TurnPhase)turnPhase).PrimaryPhase() == Data.Faction.GetAlignment().PrimaryPhase())
                {
                    Task.Run(GetAllTraversiblePaths);
                }
                HasMoved = false;
                OnPreMainPhase?.Invoke(this, new UnitEventArgs(this, (TurnPhase)turnPhase, turnCount));
            }
        }

        private void MainPhase(int turnPhase, int turnCount)
        {
            if (Exists) OnMainPhase?.Invoke(this, new UnitEventArgs(this, (TurnPhase)turnPhase, turnCount));
        }

        private void PostMainPhase(int turnPhase, int turnCount)
        {
            if (Exists) OnPostMainPhase?.Invoke(this, new UnitEventArgs(this, (TurnPhase)turnPhase, turnCount));
        }

        public override void _Process(double delta)
        {
            if (Exists)
            {
                if (fieldAttackTimer >= 0) fieldAttackTimer -= (float)delta;
                switch (state)
                {
                    case AnimState.Moving:
                        previousPosition = Data.LogicalPosition;
                        if (timeInTransit >= ClassSpec.Of(Data.Class).MoveSpeed)
                        {
                            timeInTransit = 0;
                            if (pathQueue.Count == 0)
                            {
                                state = AnimState.Neutral;
                                SetAnimation();
                                movementTask.SetResult();
                            }
                            else
                            {
                                var p = pathQueue.Dequeue();
                                Data.SetLogicalPosition(p);
                                SetOrientation();
                            }
                        }
                        timeInTransit += delta;
                        CorrectRealPosition();
                        break;
                    default:
                        break;
                }
            }
        }

        public void InitializeFromUnitData(UnitData data)
        {
            ProcessMode = ProcessModeEnum.Pausable;
            ISingleton<TurnManager>.Instance.ChangedPrimaryPhase += ChangedPrimaryPhase;
            ISingleton<TurnManager>.Instance.PreMainPhase += PreMainPhase;
            ISingleton<TurnManager>.Instance.PreMainPhase += MainPhase;
            ISingleton<TurnManager>.Instance.PostMainPhase += PostMainPhase;
            Data = data ?? Data;
            Data.UnitRef = new WeakReference<Unit>(this);
            var alignment = Data.Faction.GetAlignment();
            switch (alignment)
            {
                case Alignment.Player:
                    Controller = new PlayerUnitController();
                    Controller.Name = "Controller";
                    AddChild(Controller);
                    break;
                case Alignment.Enemy:
                case Alignment.Ally:
                case Alignment.Neutral:
                    Controller = new AIUnitController();
                    Controller.Name = "Controller";
                    AddChild(Controller);
                    break;
                default:
                    throw new Exception($"Bad Alignment: {alignment}");
            }
            LoadSpriteset();
            SetAnimation();
            CorrectRealPosition();
        }

        private void LoadSpriteset()
        {
            var path = $"gfx/2D/map/unit/{ClassSpec.Of(Data.Class).AssetID}.tres";
            SpriteFrames = ResourceLoader.Load<SpriteFrames>(path);
            Trace.Assert(SpriteFrames != null, $"No SpriteFrames asset found at {path}");
        }

        private void SetAnimation()
        {
            Stop();
            var alignment = Data.Faction.GetAlignment();
            Animation = $"{state}_{alignment}";
            GD.Print($"Set anim for {Name}");
            Frame = 0;
            Play();
        }

        private void SetOrientation()
        {
            if ((previousPosition - Data.LogicalPosition).X < 0) Orientation = Cardinal.E;
            else if ((previousPosition - Data.LogicalPosition).X > 0) Orientation = Cardinal.W;
            FlipH = Orientation == Cardinal.W;
        }

        public async Task Move(Vector2I[] path, bool instant = false)
        {
            GD.Print($"{Data.Name} moving dist: {path.Length}");
            movementTask = new TaskCompletionSource();
            if (pathQueue.Count > 0) throw new Exception("Should not be trying to move w/ non-empty pathQueue");
            if (instant)
            {
                Data.SetLogicalPosition(path[path.Length - 1]);
                CorrectRealPosition();
                movementTask.SetResult();
            } 
            else
            {
                for (int i = 0; i < path.Length; i++) pathQueue.Enqueue(path[i]);
            }
            HasMoved = true;
            state = AnimState.Moving;
            SetAnimation();
            await movementTask.Task;
            GD.Print("Done moving");
        }

        private PathfindingStruct GetAllTraversiblePaths()
        {
            IsPathfindingResultCurrent = false;
            GD.Print($"{Data.Name} begins pathfinding");
            const int LARGEST_POSSIBLE_TERRAIN_MULTI = 3; // we never *should* have a greater terrain multi than this, but if we do...
            var halfBounds = new Vector2I(LARGEST_POSSIBLE_TERRAIN_MULTI * Data.Move, LARGEST_POSSIBLE_TERRAIN_MULTI * Data.Move);
            var rect = new Rect2I(Data.LogicalPosition - halfBounds, (halfBounds * 2) + Vector2I.One);
            var costs = ClassSpec.Of(Data.Class).MovementCosts;
            astar.Region = rect;
            astar.Update();
            astar.SetAttributeScores(costs);
            astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
            astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;
            var traversible = new LinkedList<Vector2I[]>();
            var targetable = new HashSet<Vector2I>();
            for (int x = rect.Position.X; x < rect.End.X; x++)
            {
                for (int y = rect.Position.Y; y < rect.End.Y; y++)
                {
                    if (ISingleton<MapSystem>.Instance.CurrentMap.IsInBounds(x, y))
                    {
                        var rawPath = astar.GetPointPath(Data.LogicalPosition, new Vector2I(x, y));
                        var path = new Vector2I[rawPath.Length];
                        int d = 0;
                        path[0] = new Vector2I(Mathf.FloorToInt(rawPath[0].X), Mathf.FloorToInt(rawPath[0].Y));
                        for (int p = 1; p < rawPath.Length; p++)
                        {
                            path[p] = new Vector2I(Mathf.FloorToInt(rawPath[p].X), Mathf.FloorToInt(rawPath[p].Y));
                            var mod = costs[(int)ISingleton<MapSystem>.Instance.CurrentMap.GetTileAttributes(path[p].X, path[p].Y).Item1];
                            if (mod == int.MaxValue)
                            {
                                d = int.MaxValue;
                                break;
                            }
                            else d += mod;
                        }
                        if (d <= Data.Move * ProjectConstants.BaseMovementCost)
                        {
                            traversible.AddLast(path);
                            targetable.Add(path[path.Length - 1] + Vector2I.Up);
                            targetable.Add(path[path.Length - 1] + Vector2I.Down);
                            targetable.Add(path[path.Length - 1] + Vector2I.Left);
                            targetable.Add(path[path.Length - 1] + Vector2I.Right);
                        }
                    }
                }
            }
            PathfindingResult = new PathfindingStruct(traversible, targetable);
            IsPathfindingResultCurrent = true;
            GD.Print("Pathfinding done");
            return PathfindingResult;
        }

        private void CorrectRealPosition()
        {
            double x = Data.LogicalPosition.X;
            double y = Data.LogicalPosition.Y;
            if (state == AnimState.Moving && timeInTransit < ClassSpec.Of(Data.Class).MoveSpeed)
            {
                double intertileDistance = Mathf.Clamp(ClassSpec.Of(Data.Class).MoveSpeed / timeInTransit, 0, 1);
                x -= Mathf.Lerp(0, x - previousPosition.X, intertileDistance);
                y -= Mathf.Lerp(0, y - previousPosition.Y, intertileDistance);
            }
            else if (fieldAttackTimer > fieldAttackTime / 2)
            {
                double intertileDistance = fieldAttackTimer / (fieldAttackTime * 2);
                x += (Orientation.VectorI().X * intertileDistance) / 2;
            }
            else if (fieldAttackTimer > fieldAttackTime)
            {
                double intertileDistance = 1 - ((fieldAttackTimer - (fieldAttackTime / 2)) / (fieldAttackTime/2));
                x += (Orientation.VectorI().X * intertileDistance) / 2;
            }
            x *= ProjectConstants.RealSizeOfOneTile;
            y *= ProjectConstants.RealSizeOfOneTile;
            x = Mathf.RoundToInt(x);
            y = Mathf.RoundToInt(y);
            Transform2D t = Transform2D.Identity.Translated(new Vector2((float)x, (float)y));
            Transform = t;
        }

        public void DealDamage(int dmg)
        {
            var prevHp = Data.CurrentHP;
            Data.SetCurrentHP(Data.CurrentHP - dmg);
            EmitSignal(SignalName.HPChanged, prevHp, Data.CurrentHP);
        }

        public void SetActionable(bool a) => Actionable = a;

        public void Fight(Unit defender)
        {
            state = AnimState.Fighting;
            defender.state = AnimState.Fighting;
            if (Data.LogicalPosition.X > defender.Data.LogicalPosition.X || Data.LogicalPosition.Y < defender.Data.LogicalPosition.Y)
            {
                Orientation = Cardinal.W;
                defender.Orientation = Cardinal.E;
            }
        }

        private float fieldAttackTimer = 0;
        const float fieldAttackTime = 0.5f;

        private List<Unit> targets = new List<Unit>();
        private List<Vector2I> notTargets = new List<Vector2I>();

        public (IReadOnlyList<Unit>, IReadOnlyList<Vector2I>) GetTargetsForItem(Item item)
        {
            targets.Clear();
            var maxRange = item != null ? ItemSpec.Of(item.ItemID).MaxRange : 1;
            var targetFlags = item != null ? ItemSpec.Of(item.ItemID).TargetFlags : TargetFlags.Ally | TargetFlags.Player;
            for (int x = (Data.LogicalPosition + (Vector2I.Left * maxRange)).X;
                     x <= (Data.LogicalPosition + (Vector2I.Right * maxRange)).X;
                     x++)
            {
                for (int y = (Data.LogicalPosition + (Vector2I.Up * maxRange)).Y;
                     y <= (Data.LogicalPosition + (Vector2I.Down * maxRange)).Y;
                     y++)
                {
                    var coords = new Vector2I(x, y);
                    var diff = (coords - Data.LogicalPosition).Abs();
                    if (diff.X + diff.Y <= maxRange && diff.X + diff.Y > 0)
                    {
                        var target = Singleton.InstanceOf<UnitManager>()?.GetUnitAtPosition(coords);
                        var t = ((TargetFlags?)target?.Data.Faction.GetAlignment()) ?? TargetFlags.None;
                        if ((targetFlags & t) != TargetFlags.None) targets.Add(target);
                        else notTargets.Add(coords);
                    }
                }
            }
            return (targets, notTargets);
        }

        public async Task FieldAttack(Action midpointCb)
        {
            fieldAttackTimer = fieldAttackTime;
            await Task.Delay(Mathf.RoundToInt(fieldAttackTime * 500));
            midpointCb();
            await Task.Delay(Mathf.RoundToInt(fieldAttackTime * 500));
            await Task.CompletedTask;
        }
    }
}
