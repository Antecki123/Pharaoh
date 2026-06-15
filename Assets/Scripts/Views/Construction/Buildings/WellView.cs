using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Work;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class WellView : BuildingView
    {
        private SignalBus signalBus;
        private SupplyModel supplyModel;

        [Inject]
        public void Constructor(SignalBus signalBus, SupplyModel supplyModel)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;

            BuildingDefinition = BuildingDefinition.Well;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(this));
            //signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.Workplace));
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

        /*private void OnDrawGizmosSelected()
        {
            foreach (var tile in workplace.InfluencedTiles)
            {
                Gizmos.color = Color.forestGreen;
                var x = tile.x + .5f;
                var z = tile.y + .5f;
                var h = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));

                Gizmos.DrawWireSphere(new Vector3(x, h, z), .2f);
            }
        }*/
    }
}