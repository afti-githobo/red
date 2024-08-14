using Godot;
using Red.MapScene.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Red.MapScene
{
    [GlobalClass]
    public partial class TurnManager : Node2D, ILockable, ISingleton<TurnManager>
    {
        /// <summary>
        /// Dispatched whenever any phase changes to any other phase. Dispatches *after*
        /// phase-specific signals.
        /// </summary>
        /// <param name="turnPhase">An enum of type TurnPhase cast to an int. Cast it back or your code will suck!</param>
        /// <param name="turnCount">The turn number (first turn is 1)</param>
        [Signal]
        public delegate void ChangedPhaseEventHandler(int turnPhase, int turnCount);
        [Signal]
        public delegate void ChangedPrimaryPhaseEventHandler(int turnPhase, int turnCount);
        [Signal]
        public delegate void PreMainPhaseEventHandler(int turnPhase, int turnCount);
        [Signal]
        public delegate void MainPhaseEventHandler(int turnPhase, int turnCount);
        [Signal]
        public delegate void PostMainPhaseEventHandler(int turnPhase, int turnCount);

        private static TurnPhase[] turnPhaseTable = {
            TurnPhase.Player  | TurnPhase.PreMain, TurnPhase.Player  | TurnPhase.Main, TurnPhase.Player  | TurnPhase.PostMain,
            TurnPhase.Ally    | TurnPhase.PreMain, TurnPhase.Ally    | TurnPhase.Main, TurnPhase.Ally    | TurnPhase.PostMain,
            TurnPhase.Neutral | TurnPhase.PreMain, TurnPhase.Neutral | TurnPhase.Main, TurnPhase.Neutral | TurnPhase.PostMain,
            TurnPhase.Enemy   | TurnPhase.PreMain, TurnPhase.Enemy   | TurnPhase.Main, TurnPhase.Enemy   | TurnPhase.PostMain,
        };

        [Export]
        private int turnCount = 0;

        [Export]
        private int turnPhaseCounter = -1;
        [Export]
        private bool enabled;

        public TurnPhase CurrentTurnPhase => turnPhaseTable[turnPhaseCounter];

        private LinkedList<ILockable.LockStruct> _locks = new LinkedList<ILockable.LockStruct>();
        public LinkedList<ILockable.LockStruct> Locks { get => _locks; }

        public override void _Ready()
        {
            ((ISingleton<TurnManager>)this).__Ready();
        }


        public override void _ExitTree()
        {
            ((ISingleton<TurnManager>)this).__ExitTree();
        }

        public override void _Process(double delta)
        {
            if (enabled)
            {
                if (turnCount == 0 && turnPhaseCounter == -1)
                {
                    turnPhaseCounter = 0;
                    StartPrimaryPhase();
                    ExecutePreMainPhase();
                }
                if (Locks.Count == 0) EndPhase();
            }

        }

        private void AdvanceTurnPhase()
        {
            void incrementTurnPhaseCounter()
            {
                turnPhaseCounter++;
                if (turnPhaseCounter == turnPhaseTable.Length)
                {
                    turnPhaseCounter = 0;
                    turnCount++;
                }
            }

            void checkIfAnyUnitExists(IReadOnlyList<Unit> units, ref bool shouldSkipPhase)
            {
                shouldSkipPhase = true;
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].Exists)
                    {
                        shouldSkipPhase = false;
                        break;
                    }
                }
            }

            var unitManager = Singleton.InstanceOf<UnitManager>();
            var playerUnits = unitManager.GetPlayerUnits();
            var enemyUnits = unitManager.GetEnemyUnits();
            var allyUnits = unitManager.GetAllyUnits();
            var neutralUnits = unitManager.GetNeutralUnits();

            var prevTurnPhase = CurrentTurnPhase;
            incrementTurnPhaseCounter();
            while (true)
            {
                // This loop will advance until we find a phase that either: a) has units or b) matches the primary phase of the last phase.
                bool shouldSkipPhase = false;
                if (CurrentTurnPhase.PrimaryPhase() != prevTurnPhase.PrimaryPhase())
                {
                    switch (CurrentTurnPhase.PrimaryPhase())
                    {
                        case TurnPhase.Player:
                            shouldSkipPhase = unitManager.PlayerCount < 1;
                            if (!shouldSkipPhase) checkIfAnyUnitExists(playerUnits, ref shouldSkipPhase);
                            break;
                        case TurnPhase.Enemy:
                            shouldSkipPhase = unitManager.EnemyCount < 1;
                            if (!shouldSkipPhase) checkIfAnyUnitExists(enemyUnits, ref shouldSkipPhase);
                            break;
                        case TurnPhase.Ally:
                            shouldSkipPhase = unitManager.AllyCount < 1;
                            if (!shouldSkipPhase) checkIfAnyUnitExists(allyUnits, ref shouldSkipPhase);
                            break;
                        case TurnPhase.Neutral:
                            shouldSkipPhase = unitManager.NeutralCount < 1;
                            if (!shouldSkipPhase) checkIfAnyUnitExists(neutralUnits, ref shouldSkipPhase);
                            break;
                        default:
                            throw new Exception($"Bad primary turn phase {CurrentTurnPhase}. Somehow. This should really, really, really (really) never be seen");
                    }        
                }
                if (!shouldSkipPhase) break;
                incrementTurnPhaseCounter();
            }
        }

        private void EndPhase()
        {
            var prevPhase = CurrentTurnPhase;
            GD.Print("Ending phase:" + CurrentTurnPhase.PrimaryPhase() + " " + CurrentTurnPhase.SubPhase());
            AdvanceTurnPhase();
            EmitSignal(SignalName.ChangedPhase, (int)turnPhaseTable[turnPhaseCounter], turnCount);
            if (CurrentTurnPhase.PrimaryPhase() != prevPhase.PrimaryPhase()) StartPrimaryPhase();
            switch (CurrentTurnPhase.SubPhase())
            {
                case TurnPhase.PreMain:
                    ExecutePreMainPhase();
                    break;
                case TurnPhase.Main:
                    ExecuteMainPhase();
                    break;
                case TurnPhase.PostMain:
                    ExecutePostMainPhase();
                    break;
                default:
                    throw new Exception($"Bad turn subphase {CurrentTurnPhase}. Somehow. This should really, really, really (really) never be seen");
            }
        }

        private void StartPrimaryPhase()
        {
            EmitSignal(SignalName.ChangedPrimaryPhase, (int)CurrentTurnPhase, turnCount);
        }

        private void ExecutePreMainPhase()
        {
            EmitSignal(SignalName.PreMainPhase, (int)CurrentTurnPhase, turnCount);
        }

        private void ExecuteMainPhase()
        {
            EmitSignal(SignalName.MainPhase, (int)CurrentTurnPhase, turnCount);
        }

        private void ExecutePostMainPhase()
        {
            EmitSignal(SignalName.PostMainPhase, (int)CurrentTurnPhase, turnCount);
        }
    }
}