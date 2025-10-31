using Models.Economy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Work
{
    public class FarmWorkplaceModel
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public CommodityModel Commodity { get; private set; }

        public float ProcessingTime { get; private set; } = 3f;

        public int MinimumWorkersCount { get; private set; } = 1;

        public int MaxWorkersCount { get; private set; }

        public float ProcessingProgress { get; private set; }

        public float SurfaceArea { get; private set; }

        public StorageModel StorageModel { get; private set; }

        public int CarriersCount { get; private set; } = 1;

        public float Irrigating { get; private set; } = 1f;

        public IReadOnlyList<object> Workers => workers;

        private List<object> workers = new List<object>();

        public FarmWorkplaceModel(CommodityName commodityName, StorageModel storageModel, float surfaceArea)
        {
            Name = $"{commodityName}Farm";
            Commodity = new CommodityModel() { Name = commodityName, Quantity = 1 };
            MaxWorkersCount = Mathf.RoundToInt(surfaceArea / 100);
            StorageModel = storageModel;
            SurfaceArea = surfaceArea;

            StorageModel.OnValueChanged += () => OnValueChanged?.Invoke();
        }

        public bool IsAnyCommodityToTake()
        {
            foreach (var commodity in StorageModel.Storage)
            {
                if (commodity.Name == Commodity.Name && commodity.Quantity > 0)
                    return true;
            }

            return false;
        }

        public bool HasStorageRoom()
        {
            var availableSpace = StorageModel.GetAvailableSpace();
            foreach (var space in availableSpace)
            {
                if (space.Name == Commodity.Name && space.Quantity >= Commodity.Quantity)
                    return true;
            }

            return false;
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

        public void SetProcessingProgress(float value)
        {
            ProcessingProgress = value;
            OnValueChanged?.Invoke();
        }
    }
}