using App.Helpers;
using Cysharp.Threading.Tasks;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Settler.Workers;

namespace Controllers.Work
{
    public class MaterialProcessingWorkplace : IWorkplace, ISupplyTarget
    {
        public WorkplaceModel WorkplaceModel => workplaceModel;

        public Vector3 EntrancePosition { get; private set; }

        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private WorkplaceModel workplaceModel;

        private float progress = 0f;
        private float checkTimer;
        private float checkSpanInSec = 5f;

        public MaterialProcessingWorkplace(PrefabManager prefabManager, SupplyModel supplyModel, WorkplaceModel workplaceModel,
            Vector3 entrancePosition)
        {
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
            this.workplaceModel = workplaceModel;

            EntrancePosition = entrancePosition;

            // DEBUG
            for (int i = 0; i < workplaceModel.MaxWorkersCount; i++)
                workplaceModel.AddWorker(new Object());
        }

        public void Work()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0)
            {
                checkTimer = checkSpanInSec;

                if (workplaceModel.IsAnyCommodityToTake() || !workplaceModel.HasRequiredComodity())
                    _ = ScheduleTransport();
            }

            if (workplaceModel.Workers.Count < workplaceModel.MinimumWorkersCount)
                return;

            if (!workplaceModel.HasRequiredComodity())
                return;

            if (!workplaceModel.StorageModel.HasEnoughRoom(workplaceModel.ProcessedCommodity.Name, workplaceModel.ProcessedCommodity.Quantity))
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
                    _ = ScheduleTransport();
            }

            workplaceModel.SetProcessingProgress(progress);
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            var commodityName = commodity.Name;
            var existing = workplaceModel.StorageModel.Storage
                .FirstOrDefault(c => c.Name == commodityName);

            if (existing != null && existing.Quantity > 0)
            {
                int amountToTake = Mathf.Min(existing.Quantity, commodity.MaxQuantity);
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

        public Vector3 GetEntrancePosition()
        {
            return EntrancePosition;
        }

        public bool HasAvailableSpots()
        {
            return workplaceModel.MaxWorkersCount - workplaceModel.Workers.Count > 0;
        }

        public IReadOnlyCollection<CommodityModel> GetStoredCommodities()
        {
            return workplaceModel.StorageModel.Storage;
        }

        private async UniTask ScheduleTransport()
        {
            if (workplaceModel.CarriersCount == 0)
                return;

            workplaceModel.UseCarrier();

            var carrierPrefab = await AddressablesUtility.LoadAssetAsync<GameObject>("CarrierView");
            var carrier = prefabManager.Instantiate<CarrierView>(carrierPrefab);

            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(EntrancePosition, WorkplaceModel.ProcessedCommodity.Name, WorkplaceModel.ProcessedCommodity.Quantity);
            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(EntrancePosition, WorkplaceModel.RequiredCommodity.Name, WorkplaceModel.RequiredCommodity.Quantity);

            var tasks = BuildCarrierTasks();

            carrier.Init(tasks);
            carrier.OnTasksFinished += () => workplaceModel.ReturnCarrier();
        }

        private Queue<CarrierTask> BuildCarrierTasks()
        {
            var tasks = new Queue<CarrierTask>();
            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(
                EntrancePosition,
                WorkplaceModel.ProcessedCommodity.Name,
                WorkplaceModel.ProcessedCommodity.Quantity);

            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(
                EntrancePosition,
                WorkplaceModel.RequiredCommodity.Name,
                WorkplaceModel.RequiredCommodity.Quantity);

            var carriedSomethingOut = false;
            var broughtSomethingIn = false;

            if (WorkplaceModel.IsAnyCommodityToTake() && targetWithFreeSpace != null)
            {
                tasks.Enqueue(new CarrierTask(this, targetWithFreeSpace, new CommodityModel
                {
                    Name = WorkplaceModel.ProcessedCommodity.Name,
                    Quantity = WorkplaceModel.ProcessedCommodity.Quantity,
                    MaxQuantity = WorkplaceModel.ProcessedCommodity.MaxQuantity
                }));
                carriedSomethingOut = true;
            }

            if (!WorkplaceModel.HasRequiredComodity() && targetWithCommodity != null)
            {
                if (!carriedSomethingOut)
                {
                    tasks.Enqueue(new CarrierTask(this, targetWithCommodity, null));
                }
                else if (targetWithFreeSpace != null && targetWithFreeSpace != targetWithCommodity)
                {
                    tasks.Enqueue(new CarrierTask(targetWithFreeSpace, targetWithCommodity, null));
                }

                tasks.Enqueue(new CarrierTask(targetWithCommodity, this, new CommodityModel
                {
                    Name = WorkplaceModel.RequiredCommodity.Name,
                    Quantity = WorkplaceModel.RequiredCommodity.Quantity,
                    MaxQuantity = WorkplaceModel.RequiredCommodity.MaxQuantity
                }));
                broughtSomethingIn = true;
            }

            if (carriedSomethingOut && !broughtSomethingIn)
            {
                tasks.Enqueue(new CarrierTask(targetWithFreeSpace, this, null));
            }

            return tasks;
        }
    }
}