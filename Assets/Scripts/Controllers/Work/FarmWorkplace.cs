using App.Helpers;
using Cysharp.Threading.Tasks;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Settler.Workers;
using static TreeEditor.TreeGroup;

namespace Controllers.Work
{
    public class FarmWorkplace : IWorkplace, ISupplyTarget
    {
        public List<CropModel> Crops { get; private set; } = new List<CropModel>();

        public float Range { get; private set; } = 25f;

        public Vector3 Position { get; private set; }

        public Vector3 EntrancePosition { get; private set; }

        public WorkplaceModel WorkplaceModel => workplaceModel;

        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private WorkplaceModel workplaceModel;

        private float checkTimer;
        private float checkSpanInSec = 5f;

        public FarmWorkplace(PrefabManager prefabManager, SupplyModel supplyModel, WorkplaceModel workplaceModel,
            Vector3 position, Vector3 entrancePosition)
        {
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
            this.workplaceModel = workplaceModel;

            Position = position;
            EntrancePosition = entrancePosition;

            // DEBUG
            for (int i = 0; i < workplaceModel.MaxWorkersCount; i++)
                workplaceModel.AddWorker(new Object());
        }

        public bool HasAvailableSpots()
        {
            return workplaceModel.MaxWorkersCount - workplaceModel.Workers.Count > 0;
        }

        public IReservationable GetReservationable()
        {
            return workplaceModel.StorageModel;
        }

        public void Work()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0)
            {
                checkTimer = checkSpanInSec;

                if (workplaceModel.StorageModel.Storage.Any(x => x.Quantity > 0))
                {
                    ScheduleTransport();
                }
            }

            foreach (var crop in Crops)
            {
                if (crop.IsWorkScheduled)
                    continue;

                switch (crop.CropFieldState)
                {
                    case CropFieldState.WaitingForPlanting:
                        _ = SchedulePlanting(crop);
                        break;
                    case CropFieldState.Growing:
                        break;
                    case CropFieldState.ReadyToHarvest:
                        _ = ScheduleHarvest(crop);
                        break;
                    default:
                        break;
                }
            }
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

        public Vector3 GetEntrancePosition()
        {
            return EntrancePosition;
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            return workplaceModel.StorageModel.GetAvailableCommodities();
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            return workplaceModel.StorageModel.GetAvailableSpace();
        }

        private void ScheduleTransport()
        {
            if (workplaceModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(
                workplaceModel.StorageModel.Storage[0].Name,
                workplaceModel.StorageModel.Storage[0].Quantity,
                out Queue<CarrierTask> tasks);

            if (result == false)
                return;

            workplaceModel.UseCarrier();

            var carrier = prefabManager.Instantiate<CarrierView>("CarrierView");
            carrier.Init(tasks);
            carrier.OnTasksFinished += () => workplaceModel.ReturnCarrier();
        }

        private bool BuildCarrierTasks(CommodityName commodityName, int quantity, out Queue<CarrierTask> tasks)
        {
            tasks = new Queue<CarrierTask>();

            var targetWithSpace = supplyModel.GetClosestStorageWithFreeSpace(EntrancePosition, commodityName, quantity);

            if (targetWithSpace == null)
                return false;

            tasks.Enqueue(new CarrierTask(this, targetWithSpace, new CommodityModel
            {
                Name = commodityName,
                Quantity = quantity
            }));
            tasks.Enqueue(new CarrierTask(targetWithSpace, this, null));

            return true;
        }

        private async UniTask SchedulePlanting(CropModel crop)
        {
            crop.IsWorkScheduled = true;
            crop.OnWorkScheduled?.Invoke("WheatFieldPlanting");
            crop.UpdateStatus(CropFieldState.Planting);

            await UniTask.WaitForSeconds(30);

            crop.UpdateStatus(CropFieldState.Growing);
            crop.IsWorkScheduled = false;
        }

        private async UniTask ScheduleHarvest(CropModel crop)
        {
            crop.IsWorkScheduled = true;
            crop.OnWorkScheduled?.Invoke("WheatFieldHarvesting");
            crop.UpdateStatus(CropFieldState.Harvesting);

            await UniTask.WaitForSeconds(20);

            workplaceModel.StorageModel.AddCommodity(crop.ProducedCommodity);

            crop.SetGrowthProgress(0f);
            crop.UpdateStatus(CropFieldState.WaitingForPlanting);
            crop.IsWorkScheduled = false;
        }
    }
}