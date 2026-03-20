using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Construction;
using Models.Economy;
using Models.Habitation;
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
        private WorkplaceEconomyImporter economyImporter;
        private ConstructionGrid constructionGrid;

        private DistributionPointWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus, SupplyModel supplyModel, WorkplaceEconomyImporter economyImporter, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.economyImporter = economyImporter;
            this.constructionGrid = constructionGrid;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            SetupWorkplace();

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(workplace, this));
            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.DistributionPoint));
        }

        public override void DestroyBuilding()
        {
            signalBus.Fire(new WorkplaceSignals.UnregisterWorkplace(workplace));
            signalBus.Fire(new WorkplaceSignals.UnregisterSupplyTarget(workplace));

            base.DestroyBuilding();
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                signalBus.Fire(new BuildingTooltipSignals.OpenDistributionPointTooltipUI(transform, workplace.DistributionModel));
            }
        }

        private void SetupWorkplace()
        {
            var buildingDefinition = BuildingDefinition.Bazaar;
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var storageModel = new StorageModel(economyImporter.StorageData[buildingDefinition]);

            var requirementDefinition = HabitationRequirementDefinition.Food;
            var distributionModel = new DistributionPointModel(buildingDefinition, economyData, storageModel, requirementDefinition);
            workplace = new DistributionPointWorkplace(signalBus, supplyModel, distributionModel, constructionGrid, this,
                economyData.InfluenceRange);
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var tile in workplace.InfluencedTiles)
            {
                Gizmos.color = Color.forestGreen;
                var x = tile.x + .5f;
                var z = tile.y + .5f;
                var h = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));

                Gizmos.DrawWireSphere(new Vector3(x, h, z), .2f);
            }
        }
    }
}