using Models.Settler;
using UnityEngine;
using Views.Construction;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class WorkState : IState
    {
        public class Factory : PlaceholderFactory<SettlerView, WorkState> { }

        private readonly SettlerView settlerView;

        private BuildingView locationOfNeedFulfillment;

        public WorkState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
        }

        public void OnEnter()
        {
            locationOfNeedFulfillment = settlerView.SettlerModel.Emplyer.BuildingView;

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
                    settlerView.SettlerModel.StrategyState = SettlerStrategyState.Working;
                    settlerView.gameObject.SetActive(true);
                }
            }
            else
            {
                settlerView.SettlerState = SettlerState.Busy;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Working;
                settlerView.gameObject.SetActive(false);
            }
        }

        public void OnExit()
        {
            settlerView.SettlerModel.SettlerNeeds.Work.IsRestoring = false;
        }

        public void Tick()
        {
            if (Vector3.Distance(settlerView.transform.position, settlerView.MovementHandler.TargetPosition) <= .1f)
            {
                settlerView.gameObject.SetActive(false);
                settlerView.SettlerModel.SettlerNeeds.Work.IsRestoring = true;

                settlerView.SettlerState = SettlerState.Busy;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Working;
                settlerView.SettlerModel.CurrentLocation = locationOfNeedFulfillment;
            }

            if (settlerView.SettlerModel.SettlerNeeds.Work.Value >= 1.0f)
            {
                settlerView.SettlerModel.SettlerNeeds.Work.IsRestoring = false;
                settlerView.SettlerState = SettlerState.Idle;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Idle;
            }
        }

        public void FixedTick()
        {

        }
    }
}