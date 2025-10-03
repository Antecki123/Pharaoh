using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models.Economy
{
    public class StorageModel
    {
        public event Action OnValueChanged;

        public IReadOnlyList<CommodityModel> Storage => storage;

        private List<CommodityModel> storage = new List<CommodityModel>();

        public StorageModel(List<CommodityModel> storage)
        {
            this.storage = storage;
        }

        public void AddCommodity(CommodityModel commodity)
        {
            var existing = storage.FirstOrDefault(c => c.Name == commodity.Name);
            if (existing != null)
            {
                existing.Quantity += commodity.Quantity;
                OnValueChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Cannot add commodity {commodity.Name} to storage.");
            }
        }

        public void RemoveCommodity(CommodityModel commodity)
        {
            var existing = storage.FirstOrDefault(c => c.Name == commodity.Name);
            if (existing != null)
            {
                existing.Quantity -= commodity.Quantity;
                existing.Quantity = existing.Quantity <= 0 ? 0 : existing.Quantity;

                OnValueChanged?.Invoke();
            }
        }

        public bool HasEnoughRoom(CommodityName name, int quantity)
        {
            foreach (var commodity in storage)
            {
                if (commodity.Name == name && commodity.MaxQuantity - commodity.Quantity >= quantity)
                    return true;
            }

            return false;
        }
    }
}