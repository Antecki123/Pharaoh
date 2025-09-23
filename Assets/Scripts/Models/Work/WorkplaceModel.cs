using Models.Economy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models.Work
{
    public class WorkplaceModel
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public CommodityModel RequiredMaterial { get; private set; }

        public CommodityModel ProcessedMaterial { get; private set; }

        public float ProcessingTime { get; private set; }

        public int MinimumWorkersCount { get; private set; }

        public int MaxWorkersCount { get; private set; }

        public IReadOnlyList<CommodityModel> Storage => storage;

        public IReadOnlyList<object> Workers => workers;

        private List<CommodityModel> storage = new List<CommodityModel>();
        private List<object> workers = new List<object>();

        public WorkplaceModel(
            string name,
            CommodityModel requiredMaterial,
            CommodityModel processedMaterial,
            float processingTime,
            int minimumWorkersCount,
            int maxWorkersCount)
        {
            Name = name;
            RequiredMaterial = requiredMaterial;
            ProcessedMaterial = processedMaterial;
            ProcessingTime = processingTime;
            MinimumWorkersCount = minimumWorkersCount;
            MaxWorkersCount = maxWorkersCount;
        }

        public void AddCommodity(CommodityModel commodity)
        {
            var existing = storage.FirstOrDefault(c => c.Name == commodity.Name);
            if (existing != null)
                existing.Quantity += commodity.Quantity;
            else
                storage.Add(commodity);

            OnValueChanged?.Invoke();
        }

        public void RemoveCommodity(CommodityModel commodity)
        {
            var existing = storage.FirstOrDefault(c => c.Name == commodity.Name);
            if (existing != null)
            {
                existing.Quantity -= commodity.Quantity;

                if (existing.Quantity <= 0)
                    existing.Quantity = 0;

                OnValueChanged?.Invoke();
            }
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
    }
}