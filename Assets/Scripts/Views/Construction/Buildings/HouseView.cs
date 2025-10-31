using App.Signals;
using Models.Economy;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class HouseView : BuildingView
    {
        private SignalBus signalBus;

        private HabitationModel habitationModel;
        private HabitatModel habitatModel;

        [Inject]
        public void Constructor(SignalBus signalBus, HabitationModel habitationModel)
        {
            this.signalBus = signalBus;
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
                signalBus.Fire(new BuildingTooltipSignals.OpenHabitationTooltip(transform, habitatModel));
            }
        }
    }
}