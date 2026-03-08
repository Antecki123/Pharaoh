using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
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
        private WorkplaceEconomyImporter economyImporter;

        private DistributionPointWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus, SupplyModel supplyModel, WorkplaceEconomyImporter economyImporter)
        {
            this.signalBus = signalBus;
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
                signalBus.Fire(new BuildingTooltipSignals.OpenDistributionPointTooltipUI(transform, workplace.DistributionModel));
            }
        }

        private void SetupWorkplace()
        {
            var buildingDefinition = BuildingDefinition.Well;
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var storageModel = new StorageModel(new List<CommodityModel>());

            var requirementDefinition = HabitationRequirementDefinition.Water;
            var distributionModel = new DistributionPointModel(buildingDefinition, economyData, storageModel, requirementDefinition);
            workplace = new DistributionPointWorkplace(signalBus, supplyModel, distributionModel, this);
        }
    }
}