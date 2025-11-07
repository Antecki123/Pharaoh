using Controllers.Work;
using Models.Work;
using Views.Construction;
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
    }
}