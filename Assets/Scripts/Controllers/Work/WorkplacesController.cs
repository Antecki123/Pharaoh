using App.Signals;
using Models.Economy;
using Models.Work;
using System;
using Zenject;

namespace Controllers.Work
{
    public interface IWorkplace
    {
        public void Work();

        public void DestroyWorkplace();

        public IEmployer GetEmployer();
    }

    public class WorkplacesController : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly SupplyModel supplyModel;
        private readonly EmploymentModel employmentModel;

        public WorkplacesController(SignalBus signalBus, SupplyModel supplyModel, EmploymentModel employmentModel)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.employmentModel = employmentModel;
        }

        public void Initialize()
        {
            signalBus.Subscribe<WorkplaceSignals.RegisterWorkplace>(RegisterWorkplace);
            signalBus.Subscribe<WorkplaceSignals.UnregisterWorkplace>(UnregisterWorkplace);
            signalBus.Subscribe<WorkplaceSignals.RegisterSupplyTarget>(RegisterSupplyTarget);
            signalBus.Subscribe<WorkplaceSignals.UnregisterSupplyTarget>(UnregisterSupplyTarget);
        }

        public void Tick()
        {
            foreach (var workplace in employmentModel.Workplaces)
            {
                workplace.Key?.Work();
            }
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<WorkplaceSignals.RegisterWorkplace>(RegisterWorkplace);
            signalBus.Unsubscribe<WorkplaceSignals.UnregisterWorkplace>(UnregisterWorkplace);
            signalBus.Unsubscribe<WorkplaceSignals.RegisterSupplyTarget>(RegisterSupplyTarget);
            signalBus.Unsubscribe<WorkplaceSignals.UnregisterSupplyTarget>(UnregisterSupplyTarget);
        }

        private void RegisterWorkplace(WorkplaceSignals.RegisterWorkplace signal)
        {
            employmentModel.AddWorkplace(signal.Workplace, signal.BuildingView);
        }

        private void UnregisterWorkplace(WorkplaceSignals.UnregisterWorkplace signal)
        {
            employmentModel.RemoveWorkplace(signal.Workplace);
            signal.Workplace.DestroyWorkplace();
        }

        private void RegisterSupplyTarget(WorkplaceSignals.RegisterSupplyTarget signal)
        {
            supplyModel.AddSupplyTarget(signal.SupplyTarget, signal.SupplyType);
        }

        private void UnregisterSupplyTarget(WorkplaceSignals.UnregisterSupplyTarget signal)
        {
            supplyModel.RemoveSupplyTarget(signal.SupplyTarget);
        }
    }
}