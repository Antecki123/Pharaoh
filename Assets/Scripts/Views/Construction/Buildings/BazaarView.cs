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
    public class BazaarView : BuildingView
    {
        [SerializeField] private MarketStallView foodStallView;
        [SerializeField] private MarketStallView beerStallView;
        [SerializeField] private MarketStallView clothesStallView;

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private SupplyModel supplyModel;
        private WorkplaceEconomyImporter economyImporter;

        private DistributionPointWorkplace workplace;

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
            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.DistributionPoint));

            workplace.OnCreateMarketStall += CreateMarketStall;
        }

        public override void DestroyBuilding()
        {
            workplace.OnCreateMarketStall -= CreateMarketStall;

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
            var storageModel = new StorageModel(new List<CommodityModel>()
            {
                new CommodityModel() { Name = CommodityName.Food, MaxQuantity = 25 },
                new CommodityModel() { Name = CommodityName.Beer, MaxQuantity = 25 },
                new CommodityModel() { Name = CommodityName.Clothes, MaxQuantity = 25 }
            });

            var distributionModel = new DistributionPointModel(buildingDefinition, economyData, storageModel);
            workplace = new DistributionPointWorkplace(prefabManager, supplyModel, distributionModel, EntranceTransform.position);

            foreach (var commodity in distributionModel.StorageModel.Storage)
                CreateMarketStall(commodity);
        }

        private void CreateMarketStall(CommodityModel commodity)
        {
            if (commodity.Name == CommodityName.Food)
            {
                foodStallView.CreateMarketStall();
                foodStallView.gameObject.SetActive(true);
            }

            if (commodity.Name == CommodityName.Beer)
            {
                beerStallView.CreateMarketStall();
                beerStallView.gameObject.SetActive(true);
            }

            if (commodity.Name == CommodityName.Clothes)
            {
                clothesStallView.CreateMarketStall();
                clothesStallView.gameObject.SetActive(true);
            }
        }
    }
}