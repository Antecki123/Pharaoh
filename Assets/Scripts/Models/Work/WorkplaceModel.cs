using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
using System;
using System.Collections.Generic;

namespace Models.Work
{
    public class WorkplaceModel : IEmployer
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public CommodityModel RequiredCommodity { get; private set; }

        public CommodityModel ProcessedCommodity { get; private set; }

        public float ProcessingTime { get; private set; }

        public int MinimumWorkersCount { get; private set; }

        public int MaxWorkersCount { get; private set; }

        public float ProcessingProgress { get; private set; }

        public int CarriersCount { get; private set; } = 1;

        public StorageModel StorageModel { get; private set; }

        public IReadOnlyList<IEmployee> Workers => workers;

        private List<IEmployee> workers = new List<IEmployee>();

        public WorkplaceModel(BuildingDefinition buildingDefinition, WorkplaceEconomyData economyData, StorageModel storageModel)
        {
            Name = buildingDefinition.ToString();
            RequiredCommodity = economyData.RequiredCommodity != null
                ? new CommodityModel() { Name = economyData.RequiredCommodity.Value, Quantity = economyData.RequiredCommodityQuantity }
                : null;
            ProcessedCommodity = economyData.ProcessedCommodity != null
                ? new CommodityModel() { Name = economyData.ProcessedCommodity.Value, Quantity = economyData.ProcessedCommodityQuantity }
                : null;
            ProcessingTime = economyData.ProcessingTime;
            MinimumWorkersCount = economyData.MinimumWorkersCount;
            MaxWorkersCount = economyData.MaxWorkersCount;
            CarriersCount = economyData.CarriersCount;
            StorageModel = storageModel;

            StorageModel.OnValueChanged += () => OnValueChanged?.Invoke();
        }

        public bool IsAnyCommodityToTake()
        {
            foreach (var commodity in StorageModel.Storage)
            {
                if (commodity.Name == ProcessedCommodity.Name && commodity.Quantity > 0)
                    return true;
            }

            return false;
        }

        public bool HasRequiredComodity()
        {
            foreach (var commodity in StorageModel.Storage)
            {
                if (commodity.Name == RequiredCommodity.Name && commodity.Quantity >= RequiredCommodity.Quantity)
                    return true;
            }

            return false;
        }

        public bool HasStorageRoom()
        {
            var availableSpace = StorageModel.GetAvailableSpace();
            foreach (var space in availableSpace)
            {
                if (space.Name == ProcessedCommodity.Name && space.Quantity >= ProcessedCommodity.Quantity)
                    return true;
            }

            return false;
        }

        public void AddWorker(IEmployee worker)
        {
            workers.Add(worker);
            OnValueChanged?.Invoke();
        }

        public void RemoveWorker(IEmployee worker)
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

        public void SetProcessingProgress(float value)
        {
            ProcessingProgress = value;
            OnValueChanged?.Invoke();
        }

        public bool HasAvailableSpot()
        {
            return workers.Count < MaxWorkersCount;
        }

        public ICollection<IEmployee> GetWorkers()
        {
            return workers;
        }
    }
}