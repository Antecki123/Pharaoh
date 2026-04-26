using Controllers.Work;
using Models.Work;
using System;
using System.Collections.Generic;
using Views.Construction;
using Views.Settler.Workers;
using Zenject;

namespace App.Signals
{
    public class WorkplaceSignals
    {
        public WorkplaceSignals(DiContainer container)
        {
            container.DeclareSignal<RegisterWorkplace>();
            container.DeclareSignal<UnregisterWorkplace>();
            container.DeclareSignal<RegisterSupplyTarget>();
            container.DeclareSignal<UnregisterSupplyTarget>();
            container.DeclareSignal<SpawnCarrier>();
            container.DeclareSignal<ReturnCarrier>();
            container.DeclareSignal<SpawnServiceAgent>();
            container.DeclareSignal<ReturnServiceAgent>();
            container.DeclareSignal<WorklplaceDestroyed>();
        }

        public class RegisterWorkplace
        {
            public IWorkplace Workplace { get; private set; }

            public BuildingView BuildingView { get; private set; }

            public RegisterWorkplace(IWorkplace workplace, BuildingView buildingView)
            {
                Workplace = workplace;
                BuildingView = buildingView;
            }
        }

        public class UnregisterWorkplace
        {
            public IWorkplace Workplace { get; private set; }

            public UnregisterWorkplace(IWorkplace workplace)
            {
                Workplace = workplace;
            }
        }

        public class RegisterSupplyTarget
        {
            public ISupplyTarget SupplyTarget { get; private set; }

            public SupplyType SupplyType { get; private set; }

            public RegisterSupplyTarget(ISupplyTarget supplyTarget, SupplyType supplyType)
            {
                SupplyTarget = supplyTarget;
                SupplyType = supplyType;
            }
        }

        public class UnregisterSupplyTarget
        {
            public ISupplyTarget SupplyTarget { get; private set; }

            public UnregisterSupplyTarget(ISupplyTarget supplyTarget)
            {
                SupplyTarget = supplyTarget;
            }
        }

        public class SpawnCarrier
        {
            public Queue<CarrierTask> CarrierTasks { get; private set; }

            public Action OnTasksFinished { get; private set; }

            public IWorkplace Workplace { get; private set; }

            public SpawnCarrier(Queue<CarrierTask> carrierTasks, Action onTasksFinished, IWorkplace workplace)
            {
                CarrierTasks = carrierTasks;
                OnTasksFinished = onTasksFinished;
                Workplace = workplace;
            }
        }

        public class ReturnCarrier
        {
            public CarrierView Carrier { get; private set; }

            public ReturnCarrier(CarrierView carrier)
            {
                Carrier = carrier;
            }
        }

        public class SpawnServiceAgent
        {
            public ServiceAgentPayload ServiceAgentPayload { get; private set; }

            public Action OnAgentReturn { get; private set; }

            public IWorkplace Workplace { get; private set; }

            public SpawnServiceAgent(ServiceAgentPayload serviceAgentPayload, Action onAgentReturn, IWorkplace workplace)
            {
                ServiceAgentPayload = serviceAgentPayload;
                OnAgentReturn = onAgentReturn;
                Workplace = workplace;
            }
        }

        public class ReturnServiceAgent
        {
            public ServiceAgentView Agent { get; private set; }

            public ReturnServiceAgent(ServiceAgentView agent)
            {
                Agent = agent;
            }
        }

        public class WorklplaceDestroyed
        {
            public IWorkplace Workplace { get; private set; }
            public WorklplaceDestroyed(IWorkplace workplace)
            {
                Workplace = workplace;
            }
        }
    }
}