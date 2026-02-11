using Models.Settler;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class LeisureState : IState
    {
        private readonly SettlerView settlerView;

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