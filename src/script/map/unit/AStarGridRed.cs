using Godot;
using Red.Data;
using System.Collections.Generic;

namespace Red.MapScene.Units
{
    public partial class AStarGridRed : AStarGrid2D
    {
        private IReadOnlyList<int> attrScores;

        public void SetAttributeScores(IReadOnlyList<int> s) => attrScores = s;

        public override float _ComputeCost(Vector2I fromId, Vector2I toId)
        {
            if (!ISingleton<MapSystem>.Instance.CurrentMap.IsInBounds(toId.X, toId.Y)) return float.PositiveInfinity;
            var t = ISingleton<MapSystem>.Instance.CurrentMap.GetTileAttributes(toId.X, toId.Y).Item1;
            var s = attrScores[(int)t];
            if (s == int.MaxValue) return float.PositiveInfinity;
            return s / (float)ProjectConstants.BaseMovementCost;
        }
    }
}