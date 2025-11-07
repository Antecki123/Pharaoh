using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Controllers.Work
{
    public class StorageWorkplace : ISupplyTarget
    {
        public Vector3 EntrancePosition { get; private set; }

        public StorageModel StorageModel => storageModel;

        private StorageModel storageModel;

        public StorageWorkplace(StorageModel storageModel, Vector3 entrancePosition)
        {
            this.storageModel = storageModel;

            EntrancePosition = entrancePosition;
        }

        public Vector3 GetEntrancePosition()
        {
            return EntrancePosition;
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            var commodityName = commodity.Name;
            var needed = commodity.Quantity;
            var taken = 0;

            var matching = storageModel.Storage
                .Where(c => commodityName.HasFlag(c.Name) && c.Quantity > 0)
                .ToList();

            if (!matching.Any())
                return false;

            foreach (var stored in matching)
            {
                if (needed <= 0)
                    break;

                var amount = Mathf.Min(stored.Quantity, needed);
                needed -= amount;
                taken += amount;

                storageModel.RemoveCommodity(new CommodityModel
                {
                    Name = stored.Name,
                    Quantity = amount
                });
            }

            commodity.Quantity = taken;
            return taken > 0;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            storageModel.AddCommodity(commodity);
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            return storageModel.GetAvailableCommodities();
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            return storageModel.GetAvailableSpace();
        }

        public IReservationable GetReservationable()
        {
            return storageModel;
        }
    }
}