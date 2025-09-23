using Controllers.Work;
using Models.Economy;
using System;

namespace Models.Settler
{
    public class SettlerModel
    {
        public SettlerDefinition SettlerDefinition { get; set; }

        [Obsolete] public SettlerProfession Profession { get; set; }

        public HabitatModel Habitation { get; set; }

        public IWorkplace Workplace { get; set; }

        public SettlerNeeds SettlerNeeds { get; set; }
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