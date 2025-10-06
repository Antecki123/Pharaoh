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

            var stallModel = new MarketStallModel(distributionModel.StorageModel.Storage[0])
            {
                IsAvailable = true
            };
            distributionModel.AddStall(stallModel);
        }

        #region Interfaces
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

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            return distributionModel.StorageModel.GetAvailableCommodities();
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            return distributionModel.StorageModel.GetAvailableSpace();
        }

        public bool HasAvailableSpots()
        {
            return distributionModel.MaxWorkersCount - distributionModel.Workers.Count > 0;
        }

        public IReservationable GetReservationable()
        {
            return distributionModel.StorageModel;
        }

        #endregion

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
                        var quantity = stall.Commodity.MaxQuantity - stall.Commodity.Quantity;
                        ScheduleTransport(new CommodityModel() { Name = stall.Commodity.Name, Quantity = quantity });
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

        private void ScheduleTransport(CommodityModel commodity)
        {
            if (distributionModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(commodity, out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            distributionModel.UseCarrier();

            var carrier = prefabManager.Instantiate<CarrierView>("CarrierView");
            carrier.Init(tasks);
            carrier.OnTasksFinished += () => distributionModel.ReturnCarrier();
        }

        private bool BuildCarrierTasks(CommodityModel commodity, out Queue<CarrierTask> tasks)
        {
            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(EntrancePosition, commodity.Name);

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