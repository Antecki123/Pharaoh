using App.Helpers;
using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class FarmersHutView : BuildingView
    {
        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private WorkplaceEconomyImporter economyImporter;

        private FarmWorkplace workplace;

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
                //var infoPanel = prefabManager.InstantiateUI<FarmInfoUI>();
                var infoPanel = FindAnyObjectByType<FarmInfoUI>(FindObjectsInactive.Include);
                //infoPanel.Init(transform, workplace.WorkplaceModel);
            }
        }

        private void SetupWorkplace()
        {
            var buildingDefinition = BuildingDefinition.FarmersHut;
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var storage = new StorageModel(new List<CommodityModel>()
            {
                new CommodityModel() { Name = CommodityName.Wheat, Quantity = 0, MaxQuantity = 50 },
                new CommodityModel() { Name = CommodityName.Linen, Quantity = 0, MaxQuantity = 50 }
            });

            var workplaceModel = new WorkplaceModel(buildingDefinition, economyData, storage);

            workplace = new FarmWorkplace(prefabManager, supplyModel, workplaceModel, transform.position, EntranceTransform.position);
        }

        private void OnDrawGizmosSelected()
        {
            if (workplace == null)
                return;

            foreach (var crop in workplace.Crops)
            {
                Handles.DrawDottedLine(transform.position, crop.Position, 1f);
                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), crop.Position, Quaternion.identity, 1f, EventType.Repaint);
            }
        }
    }
}