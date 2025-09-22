using System;
using System.Collections.Generic;

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
            storage.Add(commodity);
            OnValueChanged?.Invoke();
        }

        public void RemoveCommodity(CommodityModel commodity)
        {
            storage.Remove(commodity);
            OnValueChanged?.Invoke();
        }
    }
}