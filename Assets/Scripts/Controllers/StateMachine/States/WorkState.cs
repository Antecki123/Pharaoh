using Models.Economy;
using Models.Settler;
using UnityEngine;
using Views.Construction;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public class WorkState : IState
    {
        private readonly SettlerView settlerView;
        private readonly EmploymentModel employmentModel;

        private BuildingView locationOfNeedFulfillment;

        public WorkState(SettlerView settlerView)
        {
            this.settlerView = settlerView;
            employmentModel = ProjectContext.Instance.Container.Resolve<EmploymentModel>();
        }

        public void OnEnter()
        {
            locationOfNeedFulfillment = employmentModel.Workplaces[settlerView.SettlerModel.Workplace];
            if (settlerView.SettlerModel.CurrentLocation != locationOfNeedFulfillment)
            {
                var calculationResult = settlerView.MovementHandler.CalculateRoute(settlerView.SettlerModel.CurrentLocation.EntranceTransform.position, locationOfNeedFulfillment.EntranceTransform.position);
                if (calculationResult)
                {
                    settlerView.IsBuisy = true;
                    settlerView.SettlerModel.StrategyState = SettlerStrategyState.Relocation;
                    settlerView.transform.position = settlerView.SettlerModel.CurrentLocation.EntranceTransform.position;
                    settlerView.gameObject.SetActive(true);
                }
            }
            else
            {
                settlerView.IsBuisy = false;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Working;
            }
        }

        public void OnExit()
        {

        }

        public void Tick()
        {
            if (Vector3.Distance(settlerView.transform.position, locationOfNeedFulfillment.EntranceTransform.position) <= .1f)
            {
                settlerView.gameObject.SetActive(false);
                settlerView.IsBuisy = false;
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Working;
                settlerView.SettlerModel.CurrentLocation = locationOfNeedFulfillment;
            }
            else
            {
                settlerView.SettlerModel.StrategyState = SettlerStrategyState.Relocation;
            }
        }

        public void FixedTick()
        {

        }
    }
}