using App.Signals;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class FarmWorkplace : IWorkplace, ISupplyTarget
    {
        public Vector3 EntrancePosition { get; private set; }

        public FarmWorkplaceModel WorkplaceModel => workplaceModel;

        private SignalBus signalBus;
        private SupplyModel supplyModel;
        private FarmWorkplaceModel workplaceModel;
        private BuildingView buildingView;

        private float progress = 0f;
        private float checkTimer;
        private float checkSpanInSec = 5f;

        public FarmWorkplace(SignalBus signalBus, SupplyModel supplyModel, FarmWorkplaceModel workplaceModel, BuildingView buildingView)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.workplaceModel = workplaceModel;
            this.buildingView = buildingView;
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

        public IReservationable GetReservationable()
        {
            return workplaceModel.StorageModel;
        }

        public IEmployer GetEmployer()
        {
            return workplaceModel;
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

        public void Work()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0)
            {
                checkTimer = checkSpanInSec;

                if (workplaceModel.IsAnyCommodityToTake())
                    ScheduleTransport();
            }

            if (workplaceModel.Workers.Count < workplaceModel.MinimumWorkersCount)
                return;

            if (!workplaceModel.HasStorageRoom())
                return;

            var efficiency = Mathf.Clamp01((float)workplaceModel.Workers.Count / workplaceModel.MaxWorkersCount) * workplaceModel.Irrigating;
            progress += (Time.deltaTime / workplaceModel.ProcessingTime) * efficiency;

            if (progress >= 1)
            {
                workplaceModel.StorageModel.AddCommodity(new CommodityModel
                {
                    Name = workplaceModel.Commodity.Name,
                    Quantity = workplaceModel.Commodity.Quantity
                });

                progress = 0;
            }

            workplaceModel.SetProcessingProgress(progress);
        }

        public void DestroyWorkplace()
        {
            signalBus.Fire(new WorkplaceSignals.WorklplaceDestroyed(this));
        }

        private void ScheduleTransport()
        {
            if (workplaceModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            workplaceModel.UseCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, () => workplaceModel.ReturnCarrier(), this));
        }

        private bool BuildCarrierTasks(out Queue<CarrierTask> tasks)
        {
            tasks = default;

            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(
                EntrancePosition,
                workplaceModel.Commodity.Name,
                workplaceModel.Commodity.Quantity);

            if (targetWithFreeSpace == null)
                return false;

            var taskBuilder = new CarrierTaskBuilder()
                .AddTaskWithReservation(this, targetWithFreeSpace, workplaceModel.Commodity, ReservationType.Space)
                .AddTask(targetWithFreeSpace, this);

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }
}