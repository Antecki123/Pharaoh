using Models.Economy;
using Models.Work;
using System;
using System.Collections.Generic;
using Views.Settler.Workers;

namespace Controllers.Work
{
    public class CarrierTaskBuilder
    {
        public IReadOnlyCollection<CarrierTask> Tasks => tasks;

        private Queue<CarrierTask> tasks = new Queue<CarrierTask>();

        public CarrierTaskBuilder AddTask(ISupplyTarget origin, ISupplyTarget target)
        {
            tasks.Enqueue(new CarrierTask(origin, target, null));
            return this;
        }

        public CarrierTaskBuilder AddTaskWithReservation(ISupplyTarget origin, ISupplyTarget target, CommodityModel commodity, ReservationType reservationType)
        {
            var reservationId = Guid.NewGuid();

            if (reservationType == ReservationType.Commodity)
            {
                origin.GetReservationable().SetCommodityReservation(reservationId, commodity);
                tasks.Enqueue(new CarrierTask(origin, target, commodity, reservationId));
            }
            else if (reservationType == ReservationType.Space)
            {
                origin.GetReservationable().SetSpaceReservation(reservationId, commodity);
                tasks.Enqueue(new CarrierTask(origin, target, commodity, reservationId));
            }

            return this;
        }
    }
}