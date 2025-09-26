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

        public ISupplyTarget GetClosestSupply(Vector3 position, SupplyType supplyType)
        {
            var closest = supplyTargets
                .Where(x => x.Value == supplyType)
                .OrderBy(x => Vector3.Distance(position, x.Key.GetEntrancePosition()))
                .First();

            return closest.Key;
        }

        public ISupplyTarget GetClosestStorageWithCommodity(Vector3 position, CommodityName commodity, int commodityQuantity)
        {
            var storages = supplyTargets.Keys
            .Where(target =>
            {
                var commodities = target.GetStoredCommodities();
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

        public ISupplyTarget GetClosestStorageWithFreeSpace(Vector3 position, CommodityName commodity, int commodityQuantity)
        {
            var storages = supplyTargets.Keys
            .Where(target =>
            {
                var commodities = target.GetStoredCommodities();
                return commodities.Any(c => c.Name == commodity && c.MaxQuantity - c.Quantity >= commodityQuantity);
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

        public void SetReservation(ISupplyTarget supply, CommodityName commodity, int commodityQuantity)
        {

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