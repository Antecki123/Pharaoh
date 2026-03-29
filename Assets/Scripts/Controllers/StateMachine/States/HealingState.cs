using Models.Settler;
using Views.Construction;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class HealingState : IState
    {
        public class Factory : PlaceholderFactory<SettlerView, HealingState> { }

        private readonly SettlerView settlerView;

        private BuildingView locationOfNeedFulfillment;

        public HealingState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
        }

        public void OnEnter()
        {
            settlerView.SettlerModel.StrategyState = SettlerStrategyState.Healing;
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Health.IsRestoring = false;
        }

        public void Tick()
        {
            settlerView.SettlerModel.SettlerNeeds.Health.IsRestoring = true;
        }

        public void FixedTick()
        {

        }
    }
}