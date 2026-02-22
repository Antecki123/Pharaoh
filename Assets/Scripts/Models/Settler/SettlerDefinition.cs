using System;

namespace Models.Settler
{
    public class SettlerDefinition
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public int BirthYear { get; private set; }

        public SettlerGender Gender { get; private set; }

        public float MovementSpeed { get; private set; } = 2.2f;

        public SettlerDefinition(Guid id, string name, int birthYear, SettlerGender gender)
        {
            Id = id;
            Name = name;
            BirthYear = birthYear;
            Gender = gender;
        }
    }
}