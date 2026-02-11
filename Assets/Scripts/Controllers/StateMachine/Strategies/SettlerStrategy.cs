using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class SettlerStrategy : Strategy
    {
        public SettlerStrategy(SettlerView settler)
        {
            aiBrain = new AiBrain();

            var resting = new RestingState(settler);
            var working = new WorkState(settler);
            //var leasure = new LeisureState(settler);
            //var pray = new PrayerState(settler);
            //var healing = new HealingState();

            AddAnyTransition(resting, () => settler.SettlerModel.SettlerNeeds.Rest.Value <= 0 && settler.SettlerState == SettlerState.Idle && settler.SettlerModel.Habitation != null);
            AddAnyTransition(working, () => settler.SettlerModel.SettlerNeeds.Work.Value <= 0 && settler.SettlerState == SettlerState.Idle && settler.SettlerModel.Workplace != null);
            //AddAnyTransition(leasure, () => settler.SettlerModel.SettlerNeeds.Entertainment.Value <= 0f && settler.SettlerState == SettlerState.Idle);
            //AddAnyTransition(pray, () => settler.SettlerModel.SettlerNeeds.Pray.Value <= 0f && settler.SettlerState == SettlerState.Idle);
            //AddAnyTransition(healing, () => settler.SettlerModel.SettlerNeeds.Health.Value <= 0f && settler.SettlerState == SettlerState.Idle);

            aiBrain.SetState(resting);
        }
    }
}