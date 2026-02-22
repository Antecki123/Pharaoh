using App.Helpers;
using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class WindmillView : BuildingView
    {
        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private WorkplaceEconomyImporter economyImporter;

        private MaterialProcessingWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus, PrefabManager prefabManager, SupplyModel supplyModel, WorkplaceEconomyImporter economyImporter)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
            this.economyImporter = economyImporter;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            SetupWorkplace();

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(workplace, this));
            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.Workplace));
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
                signalBus.Fire(new BuildingTooltipSignals.OpenProcessingWorkplaceTooltip(transform, workplace.WorkplaceModel));
            }
        }

        private void SetupWorkplace()
        {
            var buildingDefinition = BuildingDefinition.Windmill;
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var storage = new StorageModel(new List<CommodityModel>()
            {
                new CommodityModel() { Name = CommodityName.Wheat, MaxQuantity = 1 },
                new CommodityModel() { Name = CommodityName.Flour, MaxQuantity = 50 }
            });

            var workplaceModel = new WorkplaceModel(buildingDefinition, economyData, storage);

            workplace = new MaterialProcessingWorkplace(prefabManager, supplyModel, signalBus, workplaceModel, this);
        }
    }
}