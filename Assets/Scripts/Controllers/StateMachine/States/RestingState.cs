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
            locationOfNeedFulfillment = habitationModel.Habitations[settlerView.SettlerModel.Habitation];

            if (locationOfNeedFulfillment == null)
            {
                settlerView.SettlerState = SettlerState.Idle;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Idle;
                settlerView.gameObject.SetActive(false);

                return;
            }

            if (settlerView.SettlerModel.CurrentLocation != locationOfNeedFulfillment)
            {
                var calculationResult = settlerView.MovementHandler.CalculateRoute(settlerView.SettlerModel.CurrentLocation, locationOfNeedFulfillment);
                if (calculationResult)
                {
                    settlerView.SettlerState = SettlerState.Movement;
                    settlerView.SettlerModel.StrategyState = SettlerStrategyState.Resting;
                    settlerView.gameObject.SetActive(true);
                }
            }
            else
            {
                settlerView.SettlerState = SettlerState.Busy;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Resting;
                settlerView.gameObject.SetActive(false);
            }
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = false;
        }

        public void Tick()
        {
            if (Vector3.Distance(settlerView.transform.position, settlerView.MovementHandler.TargetPosition) <= .1f)
            {
                settlerView.gameObject.SetActive(false);
                settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = true;

                settlerView.SettlerState = SettlerState.Busy;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Resting;
                settlerView.SettlerModel.CurrentLocation = locationOfNeedFulfillment;
            }

            if (settlerView.SettlerModel.SettlerNeeds.Rest.Value >= 1.0f)
            {
                settlerView.SettlerModel.SettlerNeeds.Rest.IsRestoring = false;
                settlerView.SettlerState = SettlerState.Idle;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Idle;
            }
        }

        public void FixedTick()
        {

        }
    }
}