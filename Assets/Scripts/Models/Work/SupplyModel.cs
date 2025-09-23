using Controllers.Work;
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

        public void AddSupply(ISupplyTarget supplyTarget, SupplyType supplyType)
        {
            supplyTargets.Add(supplyTarget, supplyType);
            OnValueChanged?.Invoke();
        }

        public void RemoveSupply(ISupplyTarget supplyTarget)
        {
            supplyTargets.Remove(supplyTarget);
            OnValueChanged?.Invoke();
        }

        public ISupplyTarget GetClosestSupply(Vector3 position, SupplyType supplyType)
        {
            var closest = supplyTargets
                .Where(t => t.Value == supplyType)
                .OrderBy(t => Vector3.Distance(position, t.Key.GetEntrancePosition()))
                .First();

            return closest.Key;
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