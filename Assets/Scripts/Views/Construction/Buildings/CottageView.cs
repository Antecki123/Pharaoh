using App.Signals;
using Models.Economy;
using Models.Habitation;
using UnityEngine;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class CottageView : BuildingView, IServiceReceiver
    {
        private SignalBus signalBus;
        private HabitatModel habitatModel;

        private readonly int[] residentsPerLevel = { 4, 8, 16 };
        private readonly string[] namesPerLevel = { "Farmers House", "Workers House", "Engineers House" };

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;

            var foodQuantity = Random.Range(0, 100);
            var beerQuantity = Random.Range(0, 100);
            var clothesQuantity = Random.Range(0, 30);

            habitatModel = new HabitatModel(namesPerLevel, residentsPerLevel);
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Food, Quantity = foodQuantity, MaxQuantity = 100 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Beer, Quantity = beerQuantity, MaxQuantity = 100 });
            habitatModel.AddCommodity(new CommodityModel() { Name = CommodityName.Clothes, Quantity = clothesQuantity, MaxQuantity = 30 });
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

        public float SatisfyResidentNeeds(HabitationRequirementDefinition requirementDefinition, float value)
        {
            return habitatModel.SatisfyResidentNeeds(requirementDefinition, value);
        }
    }
}