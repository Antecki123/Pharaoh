using System;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class SettlerStrategy : Strategy
    {
        private SettlerView settler;

        public SettlerStrategy(SettlerView settler)
        {
            this.settler = settler;
            aiBrain = new AiBrain();

            var goToPosition = new GoToPositionState();
            var sleeping = new SleepState();
            var working = new WorkState();

            AddTransition(sleeping, working, TargetReached());
            AddTransition(working, sleeping, TargetReached());

            Func<bool> TargetReached()
            {
                return () => true;
            }
        }
    }
}