using Controllers.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models.Economy
{
    public interface IReservationable
    {
        public bool SetCommodityReservation(Guid id, CommodityModel commodity);
        public bool SetSpaceReservation(Guid id, CommodityModel commodity);
        public void RemoveReservation(Guid reservatioId);
    }

    public class StorageModel : IReservationable
    {
        public event Action OnValueChanged;

        public IReadOnlyDictionary<CommodityName, CommodityEntry> Commodities => commodities;

        private readonly Dictionary<CommodityName, CommodityEntry> commodities = new();
        private readonly Dictionary<Guid, List<CommodityReservation>> reservations = new();

        public StorageModel(List<StorageEconomyData> storageData)
        {
            foreach (var data in storageData)
            {
                var commodity = new CommodityModel()
                {
                    Name = data.Name,
                    Quantity = data.Quantity,
                    MaxQuantity = data.MaxQuantity,
                };

                commodities.Add(commodity.Name, new CommodityEntry()
                {
                    Model = commodity,
                    Visibility = data.CommodityVisibility
                });
            }
        }

        public void AddCommodity(CommodityModel commodity)
        {
            if (commodities.TryGetValue(commodity.Name, out var existing))
            {
                existing.Model.Quantity += commodity.Quantity;

                if (existing.Model.Quantity > existing.Model.MaxQuantity)
                    existing.Model.Quantity = existing.Model.MaxQuantity;

                OnValueChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Cannot add commodity {commodity.Name} to storage.");
            }
        }

        public void RemoveCommodity(CommodityModel commodity)
        {
            if (commodities.TryGetValue(commodity.Name, out var existing))
            {
                existing.Model.Quantity -= commodity.Quantity;
                existing.Model.Quantity = existing.Model.Quantity <= 0 ? 0 : existing.Model.Quantity;

                OnValueChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Cannot remove commodity {commodity.Name} from storage.");
            }
        }

        public bool HasCommodities(CommodityName commodityName)
        {
            return commodities.TryGetValue(commodityName, out var commodity) &&
                   commodity.Model.Quantity > 0;
        }

        public bool HasCommodities(CommodityName commodityName, int quantity)
        {
            return commodities.TryGetValue(commodityName, out var commodity) &&
                   commodity.Model.Quantity >= quantity;
        }

        public bool HasStorageRoom(CommodityName commodityName, int quantity)
        {
            return commodities.TryGetValue(commodityName, out var commodity) &&
                   commodity.Model.MaxQuantity - quantity > 0;
        }

        public List<CommodityModel> GetAvailableCommodities()
        {
            var result = new List<CommodityModel>();

            foreach (var commodity in commodities.Values)
            {
                int reservedQuantity = 0;

                foreach (var reservationList in reservations.Values)
                {
                    foreach (var reservation in reservationList)
                    {
                        if (reservation.ReservationType == ReservationType.Commodity &&
                            reservation.Commodity.Name == commodity.Model.Name)
                        {
                            reservedQuantity += reservation.Commodity.Quantity;
                        }
                    }
                }

                int availableQuantity = commodity.Model.Quantity - reservedQuantity;

                if (availableQuantity > 0)
                {
                    result.Add(new CommodityModel
                    {
                        Name = commodity.Model.Name,
                        Quantity = availableQuantity,
                        MaxQuantity = commodity.Model.MaxQuantity
                    });
                }
            }

            return result;
        }

        public List<CommodityModel> GetAvailableSpace()
        {
            var result = new List<CommodityModel>();

            foreach (var commodity in commodities.Values)
            {
                int reservedSpace = 0;

                foreach (var reservationList in reservations.Values)
                {
                    foreach (var reservation in reservationList)
                    {
                        if (reservation.ReservationType == ReservationType.Space &&
                            reservation.Commodity.Name == commodity.Model.Name)
                        {
                            reservedSpace += reservation.Commodity.Quantity;
                        }
                    }
                }

                int freeSpace = commodity.Model.MaxQuantity - commodity.Model.Quantity - reservedSpace;

                if (freeSpace > 0)
                {
                    result.Add(new CommodityModel
                    {
                        Name = commodity.Model.Name,
                        Quantity = freeSpace
                    });
                }
            }

            return result;
        }

        public bool SetCommodityReservation(Guid id, CommodityModel commodity)
        {
            var matchingList = commodities.Values
                .Where(c => commodity.Name.HasFlag(c.Model.Name) && c.Model.Quantity > 0)
                .ToList();

            if (matchingList.Count == 0)
                return false;

            if (!reservations.TryGetValue(id, out var reservationList))
            {
                reservationList = new List<CommodityReservation>();
                reservations[id] = reservationList;
            }

            int remaining = commodity.Quantity;
            int reserved = 0;

            foreach (var matched in matchingList)
            {
                if (remaining <= 0)
                    break;

                int take = Math.Min(matched.Model.Quantity, remaining);

                if (take > 0)
                {
                    reservationList.Add(new CommodityReservation
                    {
                        Commodity = CommodityModel.Clone(new CommodityModel
                        {
                            Name = matched.Model.Name,
                            Quantity = take
                        }),
                        ReservationType = ReservationType.Commodity
                    });

                    reserved += take;
                    remaining -= take;
                }
            }

            return reserved > 0;
        }

        public bool SetSpaceReservation(Guid id, CommodityModel commodity)
        {
            var matchingList = commodities.Values
                .Where(c => commodity.Name.HasFlag(c.Model.Name))
                .ToList();

            if (matchingList.Count == 0)
                return false;

            if (!reservations.TryGetValue(id, out var reservationList))
            {
                reservationList = new List<CommodityReservation>();
                reservations[id] = reservationList;
            }

            int remaining = commodity.Quantity;
            int reserved = 0;

            foreach (var matched in matchingList)
            {
                if (remaining <= 0)
                    break;

                int alreadyReserved = reservations.Values
                    .SelectMany(r => r)
                    .Where(r => r.ReservationType == ReservationType.Space && r.Commodity.Name == matched.Model.Name)
                    .Sum(r => r.Commodity.Quantity);

                int freeSpace = matched.Model.MaxQuantity - matched.Model.Quantity - alreadyReserved;
                int take = Math.Min(freeSpace, remaining);

                if (take > 0)
                {
                    reservationList.Add(new CommodityReservation
                    {
                        Commodity = CommodityModel.Clone(new CommodityModel
                        {
                            Name = matched.Model.Name,
                            Quantity = take
                        }),
                        ReservationType = ReservationType.Space
                    });

                    reserved += take;
                    remaining -= take;
                }
            }

            return reserved > 0;
        }

        public void RemoveReservation(Guid reservationId)
        {
            reservations.Remove(reservationId);
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            var commodityName = commodity.Name;
            var needed = commodity.Quantity;
            var taken = 0;

            var matching = Commodities.Values
                .Where(c => commodityName.HasFlag(c.Model.Name) && c.Model.Quantity > 0)
                .ToList();

            if (!matching.Any())
                return false;

            foreach (var stored in matching)
            {
                if (needed <= 0)
                    break;

                var amount = Mathf.Min(stored.Model.Quantity, needed);
                needed -= amount;
                taken += amount;

                RemoveCommodity(new CommodityModel
                {
                    Name = stored.Model.Name,
                    Quantity = amount
                });
            }

            commodity.Quantity = taken;
            return taken > 0;
        }
    }

    public class CommodityReservation
    {
        public CommodityModel Commodity { get; set; }
        public ReservationType ReservationType { get; set; }
    }

    public class CommodityEntry
    {
        public CommodityModel Model { get; set; }
        public CommodityVisibility Visibility { get; set; }
    }

    public enum ReservationType { Commodity, Space }

    public enum CommodityVisibility { Private, Public }
}