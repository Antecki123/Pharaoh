using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Work;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class BazaarView : BuildingView
    {
        private SignalBus signalBus;
        private SupplyModel supplyModel;

        private readonly BuildingDefinition buildingDefinition = BuildingDefinition.Bazaar;

        [Inject]
        public void Constructor(SignalBus signalBus, SupplyModel supplyModel)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;

            BuildingDefinition = BuildingDefinition.Bazaar;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(this));
            //signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.DistributionPoint));
        }

        public override void DestroyBuilding()
        {
            signalBus.Fire(new WorkplaceSignals.UnregisterWorkplace(this));
            //signalBus.Fire(new WorkplaceSignals.UnregisterSupplyTarget(workplace));

            base.DestroyBuilding();
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                //signalBus.Fire(new BuildingTooltipSignals.OpenDistributionPointTooltipUI(transform, workplace.DistributionModel));
            }
        }

        public override void ReceiveService(IService service)
        {
            //workplace.ReceiveService(service);
        }
    }
}