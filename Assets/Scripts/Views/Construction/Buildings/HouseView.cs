using Models.Economy;
using System.Collections.Generic;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class HouseView : BuildingView
    {
        private HabitationModel habitationModel;
        private HabitatModel habitatModel;

        [Inject]
        public void Constructor(HabitationModel habitationModel)
        {
            this.habitationModel = habitationModel;
            habitatModel = new HabitatModel("House", 16);

            var foodQuantity = Random.Range(0, 300);
            var beerQuantity = Random.Range(0, 300);
            var clothesQuantity = Random.Range(0, 50);

            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Food, Quantity = foodQuantity, MaxQuantity = 300 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Beer, Quantity = beerQuantity, MaxQuantity = 300 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Clothes, Quantity = clothesQuantity, MaxQuantity = 50 });
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
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