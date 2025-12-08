using Models.Economy;
using Models.Settler;
using UnityEngine;
using Views.Construction;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class RestingState : IState
    {
        private readonly SettlerView settlerView;
        private readonly HabitationModel habitationModel;

        private BuildingView locationOfNeedFulfillment;

        public RestingState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
            habitationModel = ProjectContext.Instance.Container.Resolve<HabitationModel>();
        }

        public void OnEnter()
        {
            settlerView.IsBuisy = true;
            locationOfNeedFulfillment = habitationModel.Habitations[settlerView.SettlerModel.Habitation];

            if (settlerView.SettlerModel.CurrentLocation != locationOfNeedFulfillment)
            {
                var calculationResult = settlerView.MovementHandler.CalculateRoute(settlerView.SettlerModel.CurrentLocation.EntranceTransform.position, locationOfNeedFulfillment.EntranceTransform.position);
                if (calculationResult)
                {
                    settlerView.transform.position = settlerView.SettlerModel.CurrentLocation.EntranceTransform.position;
                    settlerView.SettlerModel.StrategyState = SettlerStrategyState.Relocation;
                    settlerView.gameObject.SetActive(true);
                }
            }
            else
            {
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Resting;
            }
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = false;
            settlerView.IsBuisy = false;
        }

        public void Tick()
        {
            if (Vector3.Distance(settlerView.transform.position, locationOfNeedFulfillment.EntranceTransform.position) <= .1f)
            {
                settlerView.gameObject.SetActive(false);
                settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = true;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Resting;
                settlerView.SettlerModel.CurrentLocation = locationOfNeedFulfillment;
            }
            else
            {
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Relocation;
                settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = false;
            }
        }

        public void FixedTick()
        {

        }
    }
}