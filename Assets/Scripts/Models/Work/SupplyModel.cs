using Models.Economy;
using System;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;

namespace Models.Work
{
    public class SupplyModel
    {
        public event Action OnValueChanged;

        public IReadOnlyDictionary<BuildingView, StorageModel> SupplyTargets => supplyTargets;

        private readonly Dictionary<BuildingView, StorageModel> supplyTargets = new();

        public void RegisterSupplyTarget(BuildingView building, StorageModel storage)
        {
            supplyTargets.Add(building, storage);
            OnValueChanged?.Invoke();
        }

        public void RemoveSupplyTarget(BuildingView building)
        {
            supplyTargets.Remove(building);
            OnValueChanged?.Invoke();
        }

        public StorageModel GetClosestStorageWithFreeSpace(Vector3 position, CommodityName commodity, int requiredSpace,
            CommodityVisibility visibility = CommodityVisibility.Public, float maxRange = float.MaxValue)
        {
            StorageModel closest = null;
            float closestDistance = float.MaxValue;

            foreach (var storage in supplyTargets)
            {
                if (!storage.Value.Commodities.TryGetValue(commodity, out var model))
                    continue;

                if (model.Visibility != visibility)
                    continue;

                int freeSpace = model.Model.MaxQuantity - model.Model.Quantity;
                if (freeSpace < requiredSpace)
                    continue;

                float distance = Vector3.Distance(position, storage.Key.transform.position);
                if (distance > maxRange)
                    continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = storage.Value;
                }
            }
            return closest;
        }

        public StorageModel GetClosestStorageWithCommodity(Vector3 position, CommodityName commodity, int requiredQuantity,
            CommodityVisibility visibility = CommodityVisibility.Private, float maxRange = float.MaxValue)
        {
            StorageModel closest = null;
            float closestDistance = float.MaxValue;

            foreach (var storage in supplyTargets)
            {
                if (!storage.Value.Commodities.TryGetValue(commodity, out var commodityModel))
                    continue;

                if (commodityModel.Visibility != visibility)
                    continue;

                if (commodityModel.Model.Quantity < requiredQuantity)
                    continue;

                float distance = Vector3.Distance(position, storage.Key.transform.position);
                if (distance > maxRange)
                    continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = storage.Value;
                }
            }
            return closest;
        }

        public BuildingView GetBuildingView(StorageModel storage)
        {
            foreach (var target in supplyTargets)
            {
                if (storage == target.Value)
                {
                    return target.Key;
                }
            }
            return null;
        }
    }
}