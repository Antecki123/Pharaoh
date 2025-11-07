using Controllers.Work;
using Models.Economy;
using Views.Construction;
using Views.Settler;

namespace Models.Settler
{
    public class SettlerModel : IEmployee
    {
        public SettlerDefinition SettlerDefinition { get; set; }

        public HabitatModel Habitation { get; set; }

        public IWorkplace Workplace { get; set; }

        public SettlerNeeds SettlerNeeds { get; set; } = new SettlerNeeds();

        public BuildingView CurrentLocation { get; set; }

        public StrategyState StrategyState { get; set; }
    }

    public enum SettlerProfession
    {
        None = 0,
        Caravaneer = 1,
        Farmer = 2,
    }

    public enum SettlerGender
    {
        Unknown = 0,
        Male = 1,
        Female = 2
    }
}