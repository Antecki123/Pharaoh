using App.Helpers;
using App.Signals;
using Models.Economy;
using Models.Work;
using System;
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
        public event Action<CommodityModel> OnCreateMarketStall;

        private SignalBus signalBus;
        private SupplyModel supplyModel;
        private DistributionPointModel distributionModel;
        private BuildingView buildingView;

        private float checkTimer;
        private float checkSpanInSec = 5f;

        private float consumptionTimer;
        private float consumptionTimeSpan = 2f;

        public DistributionPointWorkplace(SignalBus signalBus, SupplyModel supplyModel, DistributionPointModel distributionModel, BuildingView buildingView)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.distributionModel = distributionModel;
            this.buildingView = buildingView;

            // DEBUG
            var stallModel = new MarketStallModel(distributionModel.StorageModel.Storage[0])
            {
                IsAvailable = true
            };
            distributionModel.AddStall(stallModel);
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