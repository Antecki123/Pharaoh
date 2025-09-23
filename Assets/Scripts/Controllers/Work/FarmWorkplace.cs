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
    public class FarmWorkplace : IWorkplace, ISupplyTarget
    {
        public List<CropModel> Crops { get; private set; } = new List<CropModel>();

        public float Range { get; private set; } = 25f;

        public Vector3 Position { get; private set; }

        public Vector3 EntrancePosition { get; private set; }

        public WorkplaceModel WorkplaceModel => workplaceModel;

        private PrefabManager prefabManager;
        private WorkplaceModel workplaceModel;
        private SupplyModel supplyModel;

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
        }

        public bool HasAvailableSpots()
        {
            return workplaceModel.MaxWorkersCount - workplaceModel.Workers.Count > 0;
        }

        public void Work()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0)
            {
                checkTimer = checkSpanInSec;

                if (workplaceModel.Storage.Any(x => x.Quantity > 0))
                {
                    _ = ScheduleTransport();
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

            workplaceModel.AddCommodity(crop.ProducedCommodity);

            crop.SetGrowthProgress(0f);
            crop.UpdateStatus(CropFieldState.WaitingForPlanting);
            crop.IsWorkScheduled = false;
        }

        private async UniTask ScheduleTransport()
        {
            var carrierPrefab = await AddressablesUtility.LoadAssetAsync<GameObject>("CarrierView");
            var carrier = prefabManager.Instantiate<CarrierView>(carrierPrefab);

            var target = supplyModel.GetClosestSupply(EntrancePosition, SupplyType.Storage);
            var commodity = new CommodityModel() { Name = "Wheat", Quantity = 2 };
            carrier.Init(commodity, this, target);
        }

        public Vector3 GetEntrancePosition()
        {
            return EntrancePosition;
        }

        public bool TryPickCommodity(CommodityModel commodity)
        {
            workplaceModel.RemoveCommodity(commodity);

            return true;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            workplaceModel.AddCommodity(commodity);
        }
    }
}