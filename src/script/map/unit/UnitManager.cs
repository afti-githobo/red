using Godot;
using Red.Data.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Red.MapScene.Units
{
    [GlobalClass]
    public partial class UnitManager : Node2D, ISingleton<UnitManager>
    {
        [Export]
        int maxUnits;

        List<Unit> activeUnits = new List<Unit>();
        List<Unit> unitPool = new List<Unit>();
        private Unit unitPrototype;

        private const string unitPrototypeStr = "UnitPrototype";
        private const string unitPoolStr = "Unit (Pooled)";

        private int playerCount;
        private int neutralCount;
        private int allyCount;
        private int enemyCount;

        public override void _Ready()
        {
            ((ISingleton<UnitManager>)this).__Ready();

            var children = GetChildren();
            int preloadedUnits = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Unit)
                {
                    if (unitPrototype == null && children[i].Name == unitPrototypeStr)
                        unitPrototype = (Unit)children[i];
                    else
                    {
                        var unit = (Unit)children[i];
                        activeUnits.Add(unit);
                        unit.InitializeFromUnitData(unit.Data);
                        var alignment = unit.Data.Faction.GetAlignment();
                        switch (alignment)
                        {
                            case Alignment.Player:
                                playerCount++;
                                break;
                            case Alignment.Enemy:
                                enemyCount++;
                                break;
                            case Alignment.Ally:
                                allyCount++;
                                break;
                            case Alignment.Neutral:
                                neutralCount++;
                                break;
                            default:
                                throw new Exception($"Bad Alignment: {alignment}");
                        }
                        preloadedUnits++;
                    }
                }
            }
            if (unitPrototype == null)
            {
                throw new Exception("UnitManager must have a Unit named UnitPrototype as a child!");
            }
            for (int i = unitPool.Count; i < maxUnits; i++)
            {
                var unit = (Unit)unitPrototype.Duplicate();
                unit.Name = unitPoolStr;
                unitPool.Add(unit);
                unit.Reparent(null); // Pooled units are removed from the tree
            }
        }

        public override void _ExitTree()
        {
            // Since we can have units outside of the tree...
            // This might not even be necessary but it's easier to write it than to figure out a memory leak much later on
            for (int i = 0; i < maxUnits; i++) unitPool[i].QueueFree();

            ((ISingleton<UnitManager>)this).__ExitTree();
        }

        public bool UnitPoolIsEmpty => unitPool.Count == 0;

        public Unit Spawn(UnitData data)
        {
            if (UnitPoolIsEmpty) throw new Exception("No units in unit pool!");
            var unit = unitPool[unitPool.Count - 1];
            unitPool.RemoveAt(unitPool.Count - 1);
            activeUnits.Add(unit);
            unit.Name = data.Name;
            unit.Reparent(this);
            unit.InitializeFromUnitData(data);
            var alignment = unit.Data.Faction.GetAlignment();
            switch (alignment)
            {
                case Alignment.Player:
                    playerCount++;
                    break;
                case Alignment.Enemy:
                    enemyCount++;
                    break;
                case Alignment.Ally:
                    allyCount++;
                    break;
                case Alignment.Neutral:
                    neutralCount++;
                    break;
                default:
                    throw new Exception($"Bad Alignment: {alignment}");
            }
            return unit;
        }

        public void Release(Unit unit)
        {
            var alignment = unit.Data.Faction.GetAlignment();
            switch (alignment)
            {
                case Alignment.Player:
                    playerCount--;
                    break;
                case Alignment.Enemy:
                    enemyCount--;
                    break;
                case Alignment.Ally:
                    allyCount--;
                    break;
                case Alignment.Neutral:
                    neutralCount--;
                    break;
                default:
                    throw new Exception($"Bad Alignment: {alignment}");
            }
            activeUnits.Remove(unit);
            unit.Name = unitPoolStr;
            unit.Reparent(null);
        }

        public Unit GetUnitAtPosition(Vector2I pos)
        {
            for (int i = 0; i < activeUnits.Count; i++)
            {
                if (activeUnits[i].Data.LogicalPosition == pos)
                    return activeUnits[i];
            }
            return null;
        }

        // NOTE: if we wind up having allocation/GC issues, might have to replace these LINQ queries w/ something nastier.
        // CPU time, at least, should be a non-issue as long as we don't do anything too stupid w/ them b/c hey, turn-based!

        public IReadOnlyList<Unit> GetPlayerUnits() => activeUnits.Where(unit => unit.Data.Faction.GetAlignment() == Alignment.Player).ToArray();
        public IReadOnlyList<Unit> GetEnemyUnits() => activeUnits.Where(unit => unit.Data.Faction.GetAlignment() == Alignment.Enemy).ToArray();
        public IReadOnlyList<Unit> GetAllyUnits() => activeUnits.Where(unit => unit.Data.Faction.GetAlignment() == Alignment.Ally).ToArray();
        public IReadOnlyList<Unit> GetNeutralUnits() => activeUnits.Where(unit => unit.Data.Faction.GetAlignment() == Alignment.Neutral).ToArray();

        public IReadOnlyList<Unit> GetAllUnits() => activeUnits;

        // TODO: There's gonna need to be a mechanism to recalculate these when alignments change, but we don't need that yet!

        // ALSO TODO: Once HP is implemented, these shouldn't be used raw; they should be provided as e.g playerCount - deadPlayerCount

        public int PlayerCount => playerCount;
        public int EnemyCount => enemyCount;
        public int AllyCount => allyCount;
        public int NeutralCount => neutralCount;
    }
}