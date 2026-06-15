using App.Signals;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class PastureView : BuildingView
    {
        private SignalBus signalBus;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(this));
        }

        public override void DestroyBuilding()
        {
            signalBus.Fire(new WorkplaceSignals.UnregisterWorkplace(this));
            base.DestroyBuilding();
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                //signalBus.Fire(new BuildingTooltipSignals.OpenProcessingWorkplaceTooltip(transform, workplace.WorkplaceModel));
            }
        }
    }
}