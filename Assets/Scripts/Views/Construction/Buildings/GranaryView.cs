using App.Signals;
using Controllers.Work;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class GranaryView : BuildingView, ISupplyTarget
    {
        private SignalBus signalBus;
        private StorageModel storageModel;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            SetupStorage();

            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(this, SupplyType.Storage));
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
                //var infoPanel = prefabManager.InstantiateUI<StorageInfoUI>();
                var infoPanel = FindAnyObjectByType<StorageInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, storageModel);
            }
        }

        public Vector3 GetEntrancePosition()
        {
            return EntranceTransform.position;
        }

        public IReadOnlyCollection<CommodityModel> GetStoredCommodities()
        {
            return storageModel.Storage;
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            var commodityName = commodity.Name;
            var existing = storageModel.Storage
                .FirstOrDefault(c => c.Name == commodityName);

            if (existing != null && existing.Quantity > 0)
            {
                int amountToTake = Mathf.Min(existing.Quantity, commodity.MaxQuantity);
                commodity.Quantity = amountToTake;

                storageModel.RemoveCommodity(commodity);

                return true;
            }

            return false;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            storageModel.AddCommodity(commodity);
        }

        private void SetupStorage()
        {
            storageModel = new StorageModel("Granary", new List<CommodityModel>()
            {
                new CommodityModel() { Name = CommodityName.Wheat, Quantity = 10, MaxQuantity = 100 },
                new CommodityModel() { Name = CommodityName.Flour, Quantity = 10, MaxQuantity = 100 },
                new CommodityModel() { Name = CommodityName.Bread, Quantity = 0, MaxQuantity = 100 },
            });
        }
    }
}