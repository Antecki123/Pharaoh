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

            AddTransition(resting, working, () => settler.SettlerModel.SettlerNeeds.Rest.Value >= 1.0f && settler.SettlerModel.Workplace != null);
            //AddTransition(leasure, working, () => settler.SettlerModel.SettlerNeeds.Entertainment.Value >= 1.0f && settler.SettlerModel.Workplace != null);
            //AddTransition(pray, working, () => settler.SettlerModel.SettlerNeeds.Pray.Value >= 1.0f && settler.SettlerModel.Workplace != null);

            AddAnyTransition(resting, () => settler.SettlerModel.SettlerNeeds.Rest.Value <= 0f && !settler.IsBuisy);
            //AddAnyTransition(leasure, () => settler.SettlerModel.SettlerNeeds.Entertainment.Value <= 0f && !settler.IsBuisy);
            //AddAnyTransition(pray, () => settler.SettlerModel.SettlerNeeds.Pray.Value <= 0f && !settler.IsBuisy);

            aiBrain.SetState(resting);
        }
    }
}