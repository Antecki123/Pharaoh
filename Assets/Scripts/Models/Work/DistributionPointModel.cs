using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
using System;
using System.Collections.Generic;
using Views.Construction;

namespace Models.Work
{
    public class DistributionPointModel
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public int MinimumWorkersCount { get; private set; }

        public int MaxWorkersCount { get; private set; }

        public int CarriersCount { get; private set; } = 1;

        public StorageModel StorageModel { get; private set; }

        public IReadOnlyList<MarketStallModel> MarketStalls => marketStalls;

        public IReadOnlyList<object> Workers => workers;

        private List<MarketStallModel> marketStalls = new List<MarketStallModel>();

        private List<object> workers = new List<object>();

        public DistributionPointModel(BuildingDefinition buildingDefinition, WorkplaceEconomyData economyData, StorageModel storageModel)
        {
            Name = buildingDefinition.ToString();
            MinimumWorkersCount = economyData.MinimumWorkersCount;
            MaxWorkersCount = economyData.MaxWorkersCount;
            StorageModel = storageModel;

            StorageModel.OnValueChanged += () => OnValueChanged?.Invoke();
        }

        public void AddWorker(object worker)
        {
            workers.Add(worker);
            OnValueChanged?.Invoke();
        }

        public void RemoveWorker(object worker)
        {
            workers.Remove(worker);
            OnValueChanged?.Invoke();
        }

        public void UseCarrier()
        {
            CarriersCount--;
            OnValueChanged?.Invoke();
        }

        public void ReturnCarrier()
        {
            CarriersCount++;
            OnValueChanged?.Invoke();
        }

        public void AddStall(MarketStallModel marketStall)
        {
            marketStalls.Add(marketStall);
            OnValueChanged?.Invoke();
        }

        public void RemoveStall(MarketStallModel marketStall)
        {
            marketStalls.Remove(marketStall);
            OnValueChanged?.Invoke();
        }
    }
}
