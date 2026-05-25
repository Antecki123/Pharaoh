using App.Signals;
using Controllers.Work;
using Models.Economy;
using Models.Habitation;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class HouseView : BuildingView
    {
        private SignalBus signalBus;
        private HabitatModel habitatModel;

        private readonly int[] residentsPerLevel = { 16, 32, 64 };
        private readonly string[] namesPerLevel = { "Farmers House", "Workers House", "Engineers House" };

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;

            var foodQuantity = Random.Range(0, 300);
            var beerQuantity = Random.Range(0, 300);
            var clothesQuantity = Random.Range(0, 50);

            habitatModel = new HabitatModel(namesPerLevel, residentsPerLevel);
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Food, Quantity = foodQuantity, MaxQuantity = 300 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Beer, Quantity = beerQuantity, MaxQuantity = 300 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Clothes, Quantity = clothesQuantity, MaxQuantity = 50 });
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            signalBus.Fire(new HabitationSignals.RegisterHabitat(habitatModel, this));
        }

        public override void DestroyBuilding()
        {
            base.DestroyBuilding();
            signalBus.Fire(new HabitationSignals.UnregisterHabitat(habitatModel));
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                signalBus.Fire(new BuildingTooltipSignals.OpenHabitationTooltip(transform, habitatModel));
            }
        }

        public override void ReceiveService(IService service)
        {
            habitatModel.ReceiveService(service);
        }
    }
}