using Models.Settler;
using Views.Construction;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class LeisureState : IState
    {
        public class Factory : PlaceholderFactory<SettlerView, LeisureState> { }

        private readonly SettlerView settlerView;

        private BuildingView locationOfNeedFulfillment;

        public LeisureState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
        }

        public void OnEnter()
        {
            settlerView.SettlerModel.StrategyState = SettlerStrategyState.Leasure;
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Entertainment.IsRestoring = false;
        }

        public void Tick()
        {
            settlerView.SettlerModel.SettlerNeeds.Entertainment.IsRestoring = true;
        }

        public void FixedTick()
        {

        }
    }
}