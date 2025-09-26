using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models.Economy
{
    public class StorageModel
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public IReadOnlyList<CommodityModel> Storage => storage;

        private List<CommodityModel> storage = new List<CommodityModel>();

        public StorageModel(string name, List<CommodityModel> storage)
        {
            Name = name;
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
                Debug.LogWarning($"Cannot add commodity {commodity.Name} to {Name}");
            }
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