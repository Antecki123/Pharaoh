using Models.Economy;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class CottageView : BuildingView
    {
        private HabitationModel habitationModel;
        private HabitatModel habitatModel;

        [Inject]
        public void Constructor(HabitationModel habitationModel)
        {
            this.habitationModel = habitationModel;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            var foodQuantity = Random.Range(0, 100);
            var beerQuantity = Random.Range(0, 100);
            var clothesQuantity = Random.Range(0, 30);

            habitatModel = new HabitatModel("Cottage", 5);
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Food, Quantity = foodQuantity, MaxQuantity = 100 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Beer, Quantity = beerQuantity, MaxQuantity = 100 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Clothes, Quantity = clothesQuantity, MaxQuantity = 30 });

            habitationModel.AddHabitation(habitatModel, this);
        }

        public override void DestroyBuilding()
        {
            base.DestroyBuilding();
            habitationModel.RemoveHabitation(habitatModel);
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                //var infoPanel = prefabManager.InstantiateUI<HabitationInfoUI>();
                var infoPanel = FindAnyObjectByType<HabitationInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, habitatModel);
            }
        }
    }
}