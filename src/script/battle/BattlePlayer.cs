using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Red.Battle
{
    public abstract partial class BattlePlayer : Node2D
    {
        private Queue<Func<Task>> taskQueue = new();
        private Task curTask = Task.CompletedTask;

        public static BattlePlayer Current { get; protected set; }

        public override void _Process(double delta)
        {
            if (curTask.IsCompleted)
            {
                if (taskQueue.Count > 0) curTask = taskQueue.Dequeue().Invoke();
                else ProcessMode = ProcessModeEnum.Disabled;
            }
        }

        public virtual void StartBattle(BattleData battle)
        {
            ProcessMode = ProcessModeEnum.Pausable;
            BattleRunner.Start(battle);
            taskQueue.Enqueue(Setup);
            taskQueue.Enqueue(PreBattle);
            while (BattleRunner.Step(out var step))
                taskQueue.Enqueue(() => HandleBattleStep(step));
            taskQueue.Enqueue(PostBattle);
            taskQueue.Enqueue(Cleanup);
        }

        protected abstract Task Setup();

        protected abstract Task PreBattle();

        protected abstract Task HandleBattleStep((BattleStep, int) step);

        protected abstract Task PostBattle();

        protected abstract Task Cleanup();

    }
}