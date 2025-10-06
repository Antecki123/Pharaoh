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

        public IReadOnlyList<CommodityModel> Storage => storage;

        private List<CommodityModel> storage = new List<CommodityModel>();
        private Dictionary<Guid, List<CommodityReservation>> reservations = new Dictionary<Guid, List<CommodityReservation>>();

        public StorageModel(List<CommodityModel> storage)
        {
            this.storage = storage;
        }

        public void AddCommodity(CommodityModel commodity)
        {
            for (int i = 0; i < storage.Count; i++)
            {
                if (storage[i].Name == commodity.Name)
                {
                    var existing = storage[i];
                    existing.Quantity += commodity.Quantity;

                    if (existing.Quantity > existing.MaxQuantity)
                        existing.Quantity = existing.MaxQuantity;

                    OnValueChanged?.Invoke();
                    return;
                }
            }

            Debug.LogWarning($"Cannot add commodity {commodity.Name} to storage.");
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
            else
            {
                Debug.LogWarning($"Cannot remove commodity {commodity.Name} from storage.");
            }
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            var result = new List<CommodityModel>();

            foreach (var commodity in storage)
            {
                int reservedQuantity = 0;

                foreach (var reservationList in reservations.Values)
                {
                    foreach (var reservation in reservationList)
                    {
                        if (reservation.ReservationType == ReservationType.Commodity &&
                            reservation.Commodity.Name == commodity.Name)
                        {
                            reservedQuantity += reservation.Commodity.Quantity;
                        }
                    }
                }

                int availableQuantity = commodity.Quantity - reservedQuantity;

                if (availableQuantity > 0)
                {
                    result.Add(new CommodityModel
                    {
                        Name = commodity.Name,
                        Quantity = availableQuantity,
                        MaxQuantity = commodity.MaxQuantity
                    });
                }
            }

            return result;
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            var result = new List<CommodityModel>();

            foreach (var commodity in storage)
            {
                int reservedSpace = 0;

                foreach (var reservationList in reservations.Values)
                {
                    foreach (var reservation in reservationList)
                    {
                        if (reservation.ReservationType == ReservationType.Space &&
                            reservation.Commodity.Name == commodity.Name)
                        {
                            reservedSpace += reservation.Commodity.Quantity;
                        }
                    }
                }

                int freeSpace = commodity.MaxQuantity - commodity.Quantity - reservedSpace;

                if (freeSpace > 0)
                {
                    result.Add(new CommodityModel
                    {
                        Name = commodity.Name,
                        Quantity = freeSpace
                    });
                }
            }

            return result;
        }

        public bool SetCommodityReservation(Guid id, CommodityModel commodity)
        {
            var matchingList = storage
                .Where(c => commodity.Name.HasFlag(c.Name) && c.Quantity > 0)
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

                int take = Math.Min(matched.Quantity, remaining);

                if (take > 0)
                {
                    reservationList.Add(new CommodityReservation
                    {
                        Commodity = CommodityModel.Clone(new CommodityModel
                        {
                            Name = matched.Name,
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
            var matchingList = storage
                .Where(c => commodity.Name.HasFlag(c.Name))
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
                    .Where(r => r.ReservationType == ReservationType.Space && r.Commodity.Name == matched.Name)
                    .Sum(r => r.Commodity.Quantity);

                int freeSpace = matched.MaxQuantity - matched.Quantity - alreadyReserved;

                int take = Math.Min(freeSpace, remaining);

                if (take > 0)
                {
                    reservationList.Add(new CommodityReservation
                    {
                        Commodity = CommodityModel.Clone(new CommodityModel
                        {
                            Name = matched.Name,
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
    }

    public class CommodityReservation
    {
        public CommodityModel Commodity { get; set; }

        public ReservationType ReservationType { get; set; }
    }

    public enum ReservationType { Commodity, Space }
}