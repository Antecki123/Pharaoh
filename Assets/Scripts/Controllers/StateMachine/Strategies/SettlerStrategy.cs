using System;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class SettlerStrategy : Strategy
    {
        private bool isBuisy = false;

        public SettlerStrategy(SettlerView settler)
        {
            aiBrain = new AiBrain();

            var working = new WorkState(settler);
            var sleeping = new SleepState(settler);
            var leasure = new LeisureState();
            var pray = new PrayerState();
            var healing = new HealingState();

            AddTransition(sleeping, working, () => settler.SettlerModel.SettlerNeeds.Rest.Value >= 1.0f);
            AddTransition(leasure, working, () => settler.SettlerModel.SettlerNeeds.Entertainment.Value >= 1.0f);
            AddTransition(pray, working, () => settler.SettlerModel.SettlerNeeds.Pray.Value >= 1.0f);

            AddAnyTransition(sleeping, NeedRest());
            AddAnyTransition(leasure, NeedEntertainment());
            AddAnyTransition(pray, NeedPray());

            aiBrain.SetState(sleeping);

            Func<bool> NeedRest() => () =>
            {
                return !isBuisy
                && settler.SettlerModel.SettlerNeeds.Rest.Value <= 0f;
            };

            Func<bool> NeedEntertainment() => () =>
            {
                return !isBuisy
                && settler.SettlerModel.SettlerNeeds.Entertainment.Value <= 0f;
            };

            Func<bool> NeedPray() => () =>
            {
                return !isBuisy
                && settler.SettlerModel.SettlerNeeds.Pray.Value <= 0f;
            };
        }
    }
}