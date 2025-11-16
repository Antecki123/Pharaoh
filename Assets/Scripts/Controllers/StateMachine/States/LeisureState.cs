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
            settlerView.IsBuisy = true; 
            settlerView.SettlerModel.StrategyState = SettlerStrategyState.Leasure;
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Entertainment.IsRestoring = false;
            settlerView.IsBuisy = false;
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