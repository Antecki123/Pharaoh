using System;
using System.Collections.Generic;
using System.Linq;

namespace Models.Economy
{
    public class StorageModel
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public IReadOnlyList<CommodityModel> Storage => storage;

        private List<CommodityModel> storage = new List<CommodityModel>();

        public StorageModel(string name)
        {
            Name = name;
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
    }
}