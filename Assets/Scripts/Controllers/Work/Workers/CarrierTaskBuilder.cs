using Models.Economy;
using System;
using System.Collections.Generic;

namespace Controllers.Work
{
    public class CarrierTaskBuilder
    {
        public IReadOnlyCollection<CarrierTask> Tasks => tasks;

        private Queue<CarrierTask> tasks = new Queue<CarrierTask>();

        public CarrierTaskBuilder AddTask(StorageModel origin, StorageModel target)
        {
            tasks.Enqueue(new CarrierTask(origin, target, null));
            return this;
        }

        public CarrierTaskBuilder AddTaskWithReservation(StorageModel origin, StorageModel target, CommodityModel commodity,
            ReservationType reservationType)
        {
            var reservationId = Guid.NewGuid();

            if (reservationType == ReservationType.Commodity)
            {
                origin.SetCommodityReservation(reservationId, commodity);
                tasks.Enqueue(new CarrierTask(origin, target, commodity, reservationId));
            }
            else if (reservationType == ReservationType.Space)
            {
                origin.SetSpaceReservation(reservationId, commodity);
                tasks.Enqueue(new CarrierTask(origin, target, commodity, reservationId));
            }

            return this;
        }
    }

    public class CarrierTask
    {
        public StorageModel Origin { get; }

        public StorageModel Target { get; }

        public CommodityModel Commodity { get; }

        public Guid? ReservationId { get; }

        public CarrierTask(StorageModel origin, StorageModel target, CommodityModel commodity, Guid? reservationId = null)
        {
            Origin = origin;
            Target = target;
            Commodity = commodity;
            ReservationId = reservationId;
        }
    }
}