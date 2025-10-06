using Controllers.Work;
using Models.Economy;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models.Work
{
    public class SupplyModel
    {
        public event Action OnValueChanged;

        private Dictionary<ISupplyTarget, SupplyType> supplyTargets = new Dictionary<ISupplyTarget, SupplyType>();

        public void AddSupplyTarget(ISupplyTarget supplyTarget, SupplyType supplyType)
        {
            supplyTargets.Add(supplyTarget, supplyType);
            OnValueChanged?.Invoke();
        }

        public void RemoveSupplyTarget(ISupplyTarget supplyTarget)
        {
            supplyTargets.Remove(supplyTarget);
            OnValueChanged?.Invoke();
        }

        public ISupplyTarget GetClosestStorageWithCommodity(Vector3 position, CommodityName commodity, int commodityQuantity)
        {
            var storages = supplyTargets.Keys
            .Where(target =>
            {
                var commodities = target.GetAvailableCommodities();
                return commodities.Any(c => c.Name == commodity && c.Quantity >= commodityQuantity);
            })
            .Where(target => supplyTargets[target] == SupplyType.Storage)
            .ToList();

            if (storages.Count == 0)
                return null;

            var closest = storages
               .OrderBy(storage => Vector3.Distance(position, storage.GetEntrancePosition()))
               .FirstOrDefault();

            return closest;
        }

        public ISupplyTarget GetClosestStorageWithCommodity(Vector3 position, CommodityName commodity)
        {
            var storages = supplyTargets.Keys
            .Where(target =>
            {
                var commodities = target.GetAvailableCommodities();
                return commodities.Any(c => commodity.HasFlag(c.Name) && c.Quantity > 0);
            })
            .Where(target => supplyTargets[target] == SupplyType.Storage)
            .ToList();

            if (storages.Count == 0)
                return null;

            var closest = storages
               .OrderBy(storage => Vector3.Distance(position, storage.GetEntrancePosition()))
               .FirstOrDefault();

            return closest;
        }

        public ISupplyTarget GetClosestStorageWithFreeSpace(Vector3 position, CommodityName commodity, int requiredSpace)
        {
            var storages = supplyTargets.Keys
            .Where(target => supplyTargets[target] == SupplyType.Storage)
            .Where(target =>
            {
                var commodities = target.GetAvailableSpace();
                return commodities.Any(c => c.Name == commodity && c.Quantity >= requiredSpace);
            })
            .ToList();

            if (storages.Count == 0)
                return null;

            var closest = storages
               .OrderBy(storage => Vector3.Distance(position, storage.GetEntrancePosition()))
               .FirstOrDefault();

            return closest;
        }
    }

    public enum SupplyType
    {
        None,
        Workplace,
        Storage,
        DistributionPoint
    }
}