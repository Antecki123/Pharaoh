using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class SettlerStrategy : Strategy
    {
        public class Factory : PlaceholderFactory<SettlerView, SettlerStrategy> { }

        private readonly RestingState resting;
        private readonly WorkState working;
        private readonly LeisureState leasure;
        private readonly PrayerState pray;
        private readonly HealingState healing;

        public SettlerStrategy(SettlerView settler, RestingState.Factory restingFactory, WorkState.Factory workFactory, LeisureState.Factory leasureFactory,
            PrayerState.Factory prayFactory, HealingState.Factory healingFactory)
        {
            aiBrain = new AiBrain();

            resting = restingFactory.Create(settler);
            working = workFactory.Create(settler);
            leasure = leasureFactory.Create(settler);
            pray = prayFactory.Create(settler);
            healing = healingFactory.Create(settler);

            AddAnyTransition(resting, () => settler.SettlerModel.SettlerNeeds.Rest.Value <= 0 && settler.SettlerState == SettlerState.Idle && settler.SettlerModel.Habitation != null);
            AddAnyTransition(working, () => settler.SettlerModel.SettlerNeeds.Work.Value <= 0 && settler.SettlerState == SettlerState.Idle && settler.SettlerModel.Emplyer != null);
            //AddAnyTransition(leasure, () => settler.SettlerModel.SettlerNeeds.Entertainment.Value <= 0f && settler.SettlerState == SettlerState.Idle);
            //AddAnyTransition(pray, () => settler.SettlerModel.SettlerNeeds.Pray.Value <= 0f && settler.SettlerState == SettlerState.Idle);
            //AddAnyTransition(healing, () => settler.SettlerModel.SettlerNeeds.Health.Value <= 0f && settler.SettlerState == SettlerState.Idle);

            aiBrain.SetState(resting);
        }
    }
}