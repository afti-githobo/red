using System;

namespace Red.MapScene.Units
{
    public class UnitEventArgs : EventArgs
    {
        public readonly Unit Unit;
        public readonly TurnPhase TurnPhase;
        public readonly int TurnCount;

        public UnitEventArgs(Unit unit, TurnPhase turnPhase, int turnCount)
        {
            Unit = unit;
            TurnPhase = turnPhase;
            TurnCount = turnCount;
        }
    }
}