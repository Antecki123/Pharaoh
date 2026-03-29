using Models.Settler;
using Views.Construction;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class PrayerState : IState
    {
        public class Factory : PlaceholderFactory<SettlerView, PrayerState> { }

        private readonly SettlerView settlerView;

        private BuildingView locationOfNeedFulfillment;

        public PrayerState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
        }

        public void OnEnter()
        {
            settlerView.SettlerModel.StrategyState = SettlerStrategyState.Praying;
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Pray.IsRestoring = false;
        }

        public void Tick()
        {
            settlerView.SettlerModel.SettlerNeeds.Pray.IsRestoring = true;
        }

        public void FixedTick()
        {

        }
    }
}