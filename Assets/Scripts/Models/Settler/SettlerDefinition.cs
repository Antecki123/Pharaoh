using System;
using UnityEngine;

namespace Models.Settler
{
    public class SettlerDefinition : MonoBehaviour
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public int BirthYear { get; private set; }

        public SettlerGender Gender { get; private set; }

        public SettlerDefinition(Guid id, string name, int birthYear, SettlerGender gender)
        {
            Id = id;
            Name = name;
            BirthYear = birthYear;
            Gender = gender;
        }
    }
}