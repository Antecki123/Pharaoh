using App.Signals;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class FarmWorkplace
    {
        public class Factory : PlaceholderFactory<FarmWorkplace> { }

        private SignalBus signalBus;
        private SupplyModel supplyModel;
        private FarmWorkplaceModel workplaceModel;

        private float progress = 0f;
        private float checkTimer;
        private float checkSpanInSec = 5f;

        public FarmWorkplace(SignalBus signalBus, SupplyModel supplyModel)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
        }

        public void Tick()
        {

        }

        private void Work()
        {

        }

        private void ScheduleTransport()
        {
            /*if (workplaceModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            workplaceModel.UseCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, () => workplaceModel.ReturnCarrier(), this));*/
        }

        /*private bool BuildCarrierTasks(out Queue<CarrierTask> tasks)
        {
            tasks = default;

            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(
                EntrancePosition,
                workplaceModel.Commodity.Name,
                workplaceModel.Commodity.Quantity);

            if (targetWithFreeSpace == null)
                return false;

            var taskBuilder = new CarrierTaskBuilder()
                .AddTaskWithReservation(this, targetWithFreeSpace, workplaceModel.Commodity, ReservationType.Space)
                .AddTask(targetWithFreeSpace, this);

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }*/
    }
}