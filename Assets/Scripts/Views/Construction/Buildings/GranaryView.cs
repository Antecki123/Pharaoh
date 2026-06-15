using App.Signals;
using Controllers.Work;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class GranaryView : BuildingView
    {
        private SignalBus signalBus;
        private StorageWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(this));
        }

        public override void DestroyBuilding()
        {
            signalBus.Fire(new WorkplaceSignals.UnregisterSupplyTarget(this));

            base.DestroyBuilding();
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                signalBus.Fire(new BuildingTooltipSignals.OpenStorageTooltipUI(transform, workplace.StorageModel));
            }
        }
    }
}