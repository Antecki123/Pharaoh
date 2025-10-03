using App.Helpers;
using Models.Economy;
using Models.Work;
using System;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;

namespace Controllers.Work
{
    public class DistributionPointWorkplace : IWorkplace, ISupplyTarget
    {
        public Vector3 EntrancePosition { get; private set; }

        public DistributionPointModel DistributionModel => distributionModel;

        public event Action<CommodityModel> OnCreateMarketStall;

        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private DistributionPointModel distributionModel;

        private float availableCommodityRange = 40f;

        private float checkTimer;
        private float checkSpanInSec = 5f;

        private float consumptionTimer;
        private float consumptionTimeSpan = 2f;

        public DistributionPointWorkplace(PrefabManager prefabManager, SupplyModel supplyModel, DistributionPointModel distributionModel,
            Vector3 entrancePosition)
        {
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
            this.distributionModel = distributionModel;

            EntrancePosition = entrancePosition;

            // DEBUG
            for (int i = 0; i < distributionModel.MaxWorkersCount; i++)
                distributionModel.AddWorker(new UnityEngine.Object());

            foreach (var commodity in distributionModel.StorageModel.Storage)
            {
                var stallModel = new MarketStallModel(commodity)
                {
                    IsAvailable = true
                };
                distributionModel.AddStall(stallModel);
            }
        }

        public Vector3 GetEntrancePosition()
        {
            return EntrancePosition;
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            return false;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            distributionModel.StorageModel.AddCommodity(commodity);
        }

        public IReadOnlyCollection<CommodityModel> GetStoredCommodities()
        {
            return new List<CommodityModel>();
        }

        public bool HasAvailableSpots()
        {
            return distributionModel.MaxWorkersCount - distributionModel.Workers.Count > 0;
        }

        public void Work()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0)
            {
                checkTimer = checkSpanInSec;

                foreach (var stall in distributionModel.MarketStalls)
                {
                    if (!stall.IsAvailable)
                        return;

                    if (stall.Commodity.Quantity < stall.Commodity.MaxQuantity * .25f)
                    {
                        ScheduleTransport(stall.Commodity.Name, stall.Commodity.MaxQuantity - stall.Commodity.Quantity);
                    }
                }
            }

            consumptionTimer -= Time.deltaTime;
            if (consumptionTimer < 0)
            {
                consumptionTimer = consumptionTimeSpan;

                foreach (var stall in distributionModel.MarketStalls)
                {
                    if (!stall.IsAvailable)
                        return;

                    distributionModel.StorageModel.RemoveCommodity(new CommodityModel()
                    {
                        Name = stall.Commodity.Name,
                        Quantity = 1
                    });
                }
            }
        }

        private void ScheduleTransport(CommodityName commodityName, int quantity)
        {
            if (distributionModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(commodityName, quantity, out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            distributionModel.UseCarrier();

            var carrier = prefabManager.Instantiate<CarrierView>("CarrierView");
            carrier.Init(tasks);
            carrier.OnTasksFinished += () => distributionModel.ReturnCarrier();
        }

        private bool BuildCarrierTasks(CommodityName commodityName, int quantity, out Queue<CarrierTask> tasks)
        {
            tasks = new Queue<CarrierTask>();

            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(EntrancePosition, commodityName);

            if (targetWithCommodity == null)
                return false;

            tasks.Enqueue(new CarrierTask(this, targetWithCommodity, null));
            tasks.Enqueue(new CarrierTask(targetWithCommodity, this, new CommodityModel
            {
                Name = commodityName,
                Quantity = quantity
            }));

            return true;
        }
    }
}