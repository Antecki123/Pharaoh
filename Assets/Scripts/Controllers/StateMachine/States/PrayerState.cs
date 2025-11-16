using Models.Settler;
using Views.Settler;

namespace Controllers.Ai.Strategy
{
    public class PrayerState : IState
    {
        private readonly SettlerView settlerView;

        public PrayerState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
        }

        public void OnEnter()
        {
            settlerView.IsBuisy = true;
            settlerView.SettlerModel.StrategyState = SettlerStrategyState.Praying;
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Pray.IsRestoring = false;
            settlerView.IsBuisy = false;
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