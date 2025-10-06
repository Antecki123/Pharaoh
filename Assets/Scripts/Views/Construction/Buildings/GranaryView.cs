using App.Signals;
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
    public class GranaryView : BuildingView
    {
        private SignalBus signalBus;
        private StorageWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            SetupStorage();

            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.Storage));
        }

        public override void DestroyBuilding()
        {
            signalBus.Fire(new WorkplaceSignals.UnregisterSupplyTarget(workplace));

            base.DestroyBuilding();
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                //var infoPanel = prefabManager.InstantiateUI<StorageInfoUI>();
                var infoPanel = FindAnyObjectByType<StorageInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, workplace.StorageModel);
            }
        }

        private void SetupStorage()
        {
            var storageModel = new StorageModel(new List<CommodityModel>()
            {
                new CommodityModel() { Name = CommodityName.Bread, Quantity = 10, MaxQuantity = 500 },
                new CommodityModel() { Name = CommodityName.Meat, Quantity = 20, MaxQuantity = 500 },
            });

            workplace = new StorageWorkplace(storageModel, EntranceTransform.position);
        }
    }
}