using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Construction;
using Models.Economy;
using Models.Environment;
using Models.Habitation;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class WellView : BuildingView
    {
        private SignalBus signalBus;
        private SupplyModel supplyModel;
        private InfluenceMap influenceMap;
        private WorkplaceEconomyImporter economyImporter;
        private ConstructionGrid constructionGrid;

        private DistributionPointWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus, SupplyModel supplyModel, InfluenceMap influenceMap,
            WorkplaceEconomyImporter economyImporter, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.influenceMap = influenceMap;
            this.economyImporter = economyImporter;
            this.constructionGrid = constructionGrid;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            SetupWorkplace();

            var pos = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            influenceMap.RegisterIrrigationSource(pos, 30f, 1f);

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(workplace, this));
            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.Workplace));
        }

        public override void DestroyBuilding()
        {
            var pos = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            influenceMap.UnregisterIrrigationSource(pos, 30f, 1f);

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

        public override void ReceiveService(IService service)
        {
            workplace.ReceiveService(service);
        }

        private void SetupWorkplace()
        {
            var buildingDefinition = BuildingDefinition.Well;
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var storageModel = new StorageModel(new List<CommodityModel>());
            var service = new HabitationRequirementService(HabitatRequirementDefinition.Water, 1.0f);
            var distributionModel = new DistributionPointModel(buildingDefinition, economyData, storageModel, service);

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