using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
using Models.Habitation;
using System;
using System.Collections.Generic;

namespace Models.Work
{
    public class DistributionPointModel : IEmployer
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public int MinimumWorkersCount { get; private set; }

        public int MaxWorkersCount { get; private set; }

        public int CarriersCount { get; private set; } = 1;

        public int ServiceAgentsCount { get; private set; } = 1;

        public StorageModel StorageModel { get; private set; }

        public CommodityModel DistributedCommodity { get; private set; }

        public HabitationRequirementDefinition HabitationRequirementDefinition { get; private set; }

        public IReadOnlyList<IEmployee> Workers => workers;

        private List<IEmployee> workers = new List<IEmployee>();

        public DistributionPointModel(BuildingDefinition buildingDefinition, WorkplaceEconomyData economyData, StorageModel storageModel,
            HabitationRequirementDefinition habitationRequirementDefinition)
        {
            Name = buildingDefinition.ToString();
            MinimumWorkersCount = economyData.MinimumWorkersCount;
            MaxWorkersCount = economyData.MaxWorkersCount;
            StorageModel = storageModel;
            HabitationRequirementDefinition = habitationRequirementDefinition;

            StorageModel.OnValueChanged += () => OnValueChanged?.Invoke();
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

        public void UseServiceAgent()
        {
            ServiceAgentsCount--;
            OnValueChanged?.Invoke();
        }

        public void ReturnServiceAgent()
        {
            ServiceAgentsCount++;
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
