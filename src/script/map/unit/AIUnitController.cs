using Godot;
using System.Threading.Tasks;

namespace Red.MapScene.Units
{
    public partial class AIUnitController : UnitController
    {
        public override async Task<ActionStruct> GetAction(int turnCount)
        {
            return new ActionStruct(null, true, null, null);
        }
    }
}