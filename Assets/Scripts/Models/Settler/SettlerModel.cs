using System;

namespace Models.Settler
{
    public class SettlerModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public float MovementSpeed { get; set; }

        public SettlerGender Gender { get; set; }

        public SettlerProfession Profession { get; set; }
    }

    public enum SettlerProfession
    {
        None = 0,
        Caravaneer = 1,
        Farmer = 2,
    }

    public enum SettlerGender
    {
        Male = 0,
        Female = 1,
        Unknown = 2
    }
}