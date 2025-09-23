using App.Signals;
using Controllers.Work;
using Models.Economy;
using Models.Work;
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
                //var infoPanel = prefabManager.InstantiateUI<HabitationInfoUI>();
                var infoPanel = FindAnyObjectByType<StorageInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, storageModel);
            }
        }

        public Vector3 GetEntrancePosition()
        {
            return EntranceTransform.position;
        }

        public bool TryPickCommodity(CommodityModel commodity)
        {
            storageModel.RemoveCommodity(commodity);

            return true;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            storageModel.AddCommodity(commodity);
        }

        private void SetupStorage()
        {
            storageModel = new StorageModel("Granary");
            storageModel.AddCommodity(new CommodityModel() { Name = "Wheat", Quantity = 0, MaxQuantity = 1000 });
        }
    }
}