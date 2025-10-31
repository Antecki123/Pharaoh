using App.Helpers;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Settler.Workers;

namespace Controllers.Work
{
    public class FarmWorkplaceNew : IWorkplace, ISupplyTarget
    {
        public Vector3 EntrancePosition { get; private set; }

        public FarmWorkplaceModel WorkplaceModel => workplaceModel;

        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private FarmWorkplaceModel workplaceModel;

        private float progress = 0f;
        private float checkTimer;
        private float checkSpanInSec = 5f;

        public FarmWorkplaceNew(PrefabManager prefabManager, SupplyModel supplyModel, FarmWorkplaceModel workplaceModel, Vector3 entrancePosition)
        {
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
            this.workplaceModel = workplaceModel;

            EntrancePosition = entrancePosition;

            // DEBUG
            for (int i = 0; i < workplaceModel.MaxWorkersCount; i++)
                workplaceModel.AddWorker(new Object());
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

        public Vector3 GetEntrancePosition()
        {
            return EntrancePosition;
        }

        public IReservationable GetReservationable()
        {
            return workplaceModel.StorageModel;
        }

        public bool HasAvailableSpots()
        {
            return default;
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

        private void ScheduleTransport()
        {
            if (workplaceModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            workplaceModel.UseCarrier();

            var carrier = prefabManager.Instantiate<CarrierView>("CarrierView");
            carrier.Init(tasks);
            carrier.OnTasksFinished += () => workplaceModel.ReturnCarrier();
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