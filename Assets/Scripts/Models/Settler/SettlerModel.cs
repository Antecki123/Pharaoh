using Controllers.Work;
using Models.Economy;

namespace Models.Settler
{
    public class SettlerModel
    {
        public SettlerDefinition SettlerDefinition { get; set; }

        public SettlerProfession Profession { get; set; }

        public HabitatModel Habitation { get; set; }

        public IWorkplace Workplace { get; set; }
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