using Godot;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Red.MapScene.Units
{
    [GlobalClass]
    public partial class PlayerUnitController : UnitController
    {
        TaskCompletionSource<ActionStruct> tcs;

        public override Task<ActionStruct> GetAction(int turnCount)
        {
            tcs = new TaskCompletionSource<ActionStruct>();
            return tcs.Task;
        }

        public void SetDone()
        {
            Trace.Assert(tcs != null && !tcs.Task.IsCompleted);
            tcs.SetResult(new ActionStruct(null, true, null, null));
        }

        public void SetMove(Vector2I[] path)
        {
            Trace.Assert(tcs != null && !tcs.Task.IsCompleted);
            tcs.SetResult(new ActionStruct(path, false, null, null));
        }
    }
}