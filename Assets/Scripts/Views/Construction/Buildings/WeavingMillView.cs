using App.Signals;
using Controllers.Construction;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class WeavingMillView : BuildingView
    {
        private SignalBus signalBus;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;

            BuildingDefinition = BuildingDefinition.WeavingMill;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(this));
            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(this));
        }

        public override void DestroyBuilding()
        {
            signalBus.Fire(new WorkplaceSignals.UnregisterWorkplace(this));
            signalBus.Fire(new WorkplaceSignals.UnregisterSupplyTarget(this));

            base.DestroyBuilding();
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                /*signalBus.Fire(new BuildingTooltipSignals.OpenProcessingWorkplaceTooltip(
                    transform, workplace.WorkplaceModel));*/
            }
        }
    }
}