using App.Helpers;
using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class BakeryView : BuildingView
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

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(workplace));
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
                //var infoPanel = prefabManager.InstantiateUI<ProcessingWorkplaceInfoUI>();
                var infoPanel = FindAnyObjectByType<ProcessingWorkplaceInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, workplace.WorkplaceModel);
            }
        }

        private void SetupWorkplace()
        {
            var buildingDefinition = BuildingDefinition.Bakery;
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var storage = new StorageModel(new List<CommodityModel>()
            {
                new CommodityModel() { Name = CommodityName.Flour, MaxQuantity = 1 },
                new CommodityModel() { Name = CommodityName.Bread, MaxQuantity = 10 }
            });

            var workplaceModel = new WorkplaceModel(buildingDefinition, economyData, storage);

            workplace = new MaterialProcessingWorkplace(prefabManager, supplyModel, workplaceModel, EntranceTransform.position);
        }
    }
}