using System;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class SettlerStrategy : Strategy
    {
        public SettlerStrategy(SettlerView settler)
        {
            aiBrain = new AiBrain();

            var working = new WorkState(settler);
            var resting = new RestingState(settler);
            //var leasure = new LeisureState(settler);
            //var pray = new PrayerState(settler);
            //var healing = new HealingState();

            AddTransition(resting, working, () => settler.SettlerModel.SettlerNeeds.Rest.Value >= 1.0f);
            //AddTransition(leasure, working, () => settler.SettlerModel.SettlerNeeds.Entertainment.Value >= 1.0f);
            //AddTransition(pray, working, () => settler.SettlerModel.SettlerNeeds.Pray.Value >= 1.0f);

            AddAnyTransition(resting, NeedRest());
            //AddAnyTransition(leasure, NeedEntertainment());
            //AddAnyTransition(pray, NeedPray());

            aiBrain.SetState(working);

            Func<bool> NeedRest() => () =>
            {
                return settler.SettlerModel.SettlerNeeds.Rest.Value <= 0f && !settler.IsBuisy;
            };

            Func<bool> NeedEntertainment() => () =>
            {
                return settler.SettlerModel.SettlerNeeds.Entertainment.Value <= 0f && !settler.IsBuisy;
            };

            Func<bool> NeedPray() => () =>
            {
                return settler.SettlerModel.SettlerNeeds.Pray.Value <= 0f && !settler.IsBuisy;
            };
        }
    }
}