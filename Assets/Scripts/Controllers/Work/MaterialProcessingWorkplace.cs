using App.Helpers;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;

namespace Controllers.Work
{
    public class MaterialProcessingWorkplace : IWorkplace, ISupplyTarget
    {
        public WorkplaceModel WorkplaceModel => workplaceModel;

        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private WorkplaceModel workplaceModel;
        private BuildingView buildingView;

        private float progress = 0f;
        private float checkTimer;
        private float checkSpanInSec = 5f;

        public MaterialProcessingWorkplace(PrefabManager prefabManager, SupplyModel supplyModel, WorkplaceModel workplaceModel, BuildingView buildingView)
        {
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
            this.workplaceModel = workplaceModel;
            this.buildingView = buildingView;
        }

        public void Work()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0)
            {
                checkTimer = checkSpanInSec;

                if (workplaceModel.IsAnyCommodityToTake() || !workplaceModel.HasRequiredComodity())
                    ScheduleTransport();
            }

            if (workplaceModel.Workers.Count < workplaceModel.MinimumWorkersCount)
                return;

            if (!workplaceModel.HasRequiredComodity() || !workplaceModel.HasStorageRoom())
                return;

            var efficiency = Mathf.Clamp01((float)workplaceModel.Workers.Count / workplaceModel.MaxWorkersCount);
            progress += (Time.deltaTime / workplaceModel.ProcessingTime) * efficiency;

            if (progress >= 1)
            {
                workplaceModel.StorageModel.RemoveCommodity(new CommodityModel
                {
                    Name = workplaceModel.RequiredCommodity.Name,
                    Quantity = workplaceModel.RequiredCommodity.Quantity
                });

                workplaceModel.StorageModel.AddCommodity(new CommodityModel
                {
                    Name = workplaceModel.ProcessedCommodity.Name,
                    Quantity = workplaceModel.ProcessedCommodity.Quantity
                });

                progress = 0;

                if (workplaceModel.IsAnyCommodityToTake() || !workplaceModel.HasRequiredComodity())
                    ScheduleTransport();
            }

            workplaceModel.SetProcessingProgress(progress);
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

        public void DeliverCommodity(CommodityModel commodity)
        {
            workplaceModel.StorageModel.AddCommodity(commodity);
        }

        public BuildingView GetBuildingView()
        {
            return buildingView;
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            return workplaceModel.StorageModel.GetAvailableCommodities();
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            return workplaceModel.StorageModel.GetAvailableSpace();
        }

        public IReservationable GetReservationable()
        {
            return workplaceModel.StorageModel;
        }

        public IEmployer GetEmployer()
        {
            return workplaceModel;
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
                Vector3.zero,
                workplaceModel.ProcessedCommodity.Name,
                workplaceModel.ProcessedCommodity.Quantity);

            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(
               Vector3.zero,
               workplaceModel.RequiredCommodity.Name,
               workplaceModel.RequiredCommodity.Quantity);

            if (targetWithFreeSpace == null && targetWithCommodity == null)
                return false;

            var taskBuilder = new CarrierTaskBuilder();

            if (workplaceModel.IsAnyCommodityToTake() && workplaceModel.HasRequiredComodity())
            {
                if (targetWithFreeSpace != null)
                    taskBuilder
                    .AddTaskWithReservation(this, targetWithFreeSpace, workplaceModel.ProcessedCommodity, ReservationType.Space)
                    .AddTask(targetWithFreeSpace, this);
                else
                    return false;
            }

            else if (!workplaceModel.IsAnyCommodityToTake() && !workplaceModel.HasRequiredComodity())
            {
                if (targetWithCommodity != null)
                    taskBuilder
                        .AddTask(this, targetWithCommodity)
                        .AddTaskWithReservation(targetWithCommodity, this, workplaceModel.RequiredCommodity, ReservationType.Commodity);
                else
                    return false;
            }

            else if (workplaceModel.IsAnyCommodityToTake() && !workplaceModel.HasRequiredComodity())
            {
                if (targetWithFreeSpace != null && targetWithCommodity != null)
                    taskBuilder
                    .AddTaskWithReservation(this, targetWithFreeSpace, workplaceModel.ProcessedCommodity, ReservationType.Space)
                    .AddTask(targetWithFreeSpace, targetWithCommodity)
                    .AddTaskWithReservation(targetWithCommodity, this, workplaceModel.RequiredCommodity, ReservationType.Commodity);
                else
                    return false;
            }

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }
}