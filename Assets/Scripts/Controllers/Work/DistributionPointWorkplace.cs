using App.Signals;
using Models.Economy;
using Models.Helpers;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class DistributionPointWorkplace : IWorkplace, ISupplyTarget
    {
        public DistributionPointModel DistributionModel => distributionModel;

        private readonly SignalBus signalBus;
        private readonly SupplyModel supplyModel;
        private readonly DistributionPointModel distributionModel;
        private readonly BuildingView buildingView;

        private Timer resourceRefreshTimer;

        public DistributionPointWorkplace(SignalBus signalBus, SupplyModel supplyModel, DistributionPointModel distributionModel,
            BuildingView buildingView)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.distributionModel = distributionModel;
            this.buildingView = buildingView;

            resourceRefreshTimer = new Timer(5f);
        }

        public BuildingView GetBuildingView()
        {
            return buildingView;
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            return false;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            distributionModel.StorageModel.AddCommodity(commodity);
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            return distributionModel.StorageModel.GetAvailableCommodities();
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            return distributionModel.StorageModel.GetAvailableSpace();
        }

        public IReservationable GetReservationable()
        {
            return distributionModel.StorageModel;
        }

        public IEmployer GetEmployer()
        {
            return distributionModel;
        }

        public void Work()
        {
            resourceRefreshTimer.Tick(Time.deltaTime);

            if (!resourceRefreshTimer.IsFinished)
                return;

            if (distributionModel.DistributedCommodity == null)
            {
                DistributeResources();
            }
            else
            {
                if (distributionModel.DistributedCommodity.Quantity > 0)
                    DistributeResources();
                else
                    ScheduleTransport(distributionModel.DistributedCommodity);
            }
        }

        private void DistributeResources()
        {
            if (distributionModel.ServiceAgentsCount == 0)
                return;

            distributionModel.UseServiceAgent();
            signalBus.Fire(new WorkplaceSignals.SpawnServiceAgent(buildingView, () =>
            {
                resourceRefreshTimer.Reset();
                distributionModel.ReturnServiceAgent();
            }));
        }

        private void ScheduleTransport(CommodityModel commodity)
        {
            if (distributionModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(commodity, out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            distributionModel.UseCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, () => distributionModel.ReturnCarrier()));
        }

        private bool BuildCarrierTasks(CommodityModel commodity, out Queue<CarrierTask> tasks)
        {
            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(buildingView.transform.position, commodity.Name);

            if (targetWithCommodity == null)
            {
                tasks = default;
                return false;
            }

            var taskBuilder = new CarrierTaskBuilder();
            taskBuilder
                .AddTask(this, targetWithCommodity)
                .AddTaskWithReservation(targetWithCommodity, this, commodity, ReservationType.Commodity);

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }
}