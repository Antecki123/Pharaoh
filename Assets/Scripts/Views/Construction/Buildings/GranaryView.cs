using Models.Economy;
using UnityEngine;
using Views.Ui.Buildings;

namespace Views.Construction
{
    [SelectionBase]
    public class GranaryView : BuildingView
    {
        private StorageModel storageModel;

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            var wheatQuantity = Random.Range(0, 1000);

            storageModel = new StorageModel("Granary");
            storageModel.AddCommodity(new CommodityModel() { Name = "Wheat", Quantity = wheatQuantity, MaxQuantity = 1000 });
        }

        public override void DestroyBuilding()
        {
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
    }
}