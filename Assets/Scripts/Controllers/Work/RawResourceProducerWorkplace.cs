using App.Signals;
using Models.Economy;
using Models.Helpers;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class RawResourceProducerWorkplace : IWorkplace, ISupplyTarget
    {
        public WorkplaceModel WorkplaceModel => workplaceModel;

        private SignalBus signalBus;
        private SupplyModel supplyModel;
        private WorkplaceModel workplaceModel;
        private BuildingView buildingView;

        private Timer checkTimer;
        private float progress = 0f;

        public RawResourceProducerWorkplace(SignalBus signalBus, SupplyModel supplyModel, WorkplaceModel workplaceModel, BuildingView buildingView)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.workplaceModel = workplaceModel;
            this.buildingView = buildingView;

            checkTimer = new Timer(3f);
        }

        public void Work()
        {
            checkTimer.Tick(Time.deltaTime);
            if (checkTimer.IsFinished)
            {
                checkTimer.Reset();

                if (workplaceModel.IsAnyCommodityToTake())
                    ScheduleTransport();
            }

            if (workplaceModel.Workers.Count < workplaceModel.MinimumWorkersCount)
                return;

            if (!workplaceModel.HasStorageRoom())
                return;

            var efficiency = Mathf.Clamp01((float)workplaceModel.Workers.Count / workplaceModel.MaxWorkersCount);
            progress += (Time.deltaTime / workplaceModel.ProcessingTime) * efficiency;

            if (progress >= 1)
            {
                workplaceModel.StorageModel.AddCommodity(new CommodityModel
                {
                    Name = workplaceModel.ProcessedCommodity.Name,
                    Quantity = workplaceModel.ProcessedCommodity.Quantity
                });

                progress = 0;

                if (workplaceModel.IsAnyCommodityToTake())
                    ScheduleTransport();
            }

            workplaceModel.SetProcessingProgress(progress);
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            workplaceModel.StorageModel.AddCommodity(commodity);
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            return workplaceModel.StorageModel.GetAvailableCommodities();
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            return workplaceModel.StorageModel.GetAvailableSpace();
        }

        public BuildingView GetBuildingView()
        {
            return buildingView;
        }

        public IEmployer GetEmployer()
        {
            return workplaceModel;
        }

        public IReservationable GetReservationable()
        {
            return workplaceModel.StorageModel;
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            var commodityName = commodity.Name;
            var existing = workplaceModel.StorageModel.Storage.FirstOrDefault(c => c.Name == commodityName);

            if (existing != null && existing.Quantity > 0)
            {
                int amountToTake = Mathf.Min(existing.Quantity, commodity.Quantity);
                commodity.Quantity = amountToTake;

                workplaceModel.StorageModel.RemoveCommodity(commodity);

                return true;
            }

            return false;
        }

        private void ScheduleTransport()
        {
            if (workplaceModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            workplaceModel.UseCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, () => workplaceModel.ReturnCarrier()));
        }

        private bool BuildCarrierTasks(out Queue<CarrierTask> tasks)
        {
            tasks = default;

            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(
                buildingView.transform.position,
                workplaceModel.ProcessedCommodity.Name,
                workplaceModel.ProcessedCommodity.Quantity);

            if (targetWithFreeSpace == null)
                return false;

            var taskBuilder = new CarrierTaskBuilder()
                .AddTaskWithReservation(this, targetWithFreeSpace, workplaceModel.ProcessedCommodity, ReservationType.Space)
                .AddTask(targetWithFreeSpace, this);

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }
}