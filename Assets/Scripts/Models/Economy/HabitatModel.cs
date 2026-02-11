using Models.Settler;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Economy
{
    public class HabitatModel
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public int MaxResidents { get; private set; }

        public IReadOnlyList<SettlerModel> Residents => residents;

        public IReadOnlyList<CommodityModel> Storage => storage;

        private readonly List<SettlerModel> residents = new List<SettlerModel>();
        private readonly List<CommodityModel> storage = new List<CommodityModel>();

        public HabitatModel(string name, int maxResidents)
        {
            Name = name;
            MaxResidents = maxResidents;
        }

        public void AddResident(SettlerModel settler)
        {
            residents.Add(settler);
            OnValueChanged?.Invoke();
        }

        public void RemoveResident(SettlerModel settler)
        {
            residents.Remove(settler);
            OnValueChanged?.Invoke();
        }

        public void AddCommodity(CommodityModel commodity)
        {
            storage.Add(commodity);
            OnValueChanged?.Invoke();
        }

        public void RemoveCommodity(CommodityModel commodity)
        {
            storage.Remove(commodity);
            OnValueChanged?.Invoke();
        }

        public bool HasAvailableSpot()
        {
            return MaxResidents - residents.Count > 0;
        }
    }

    public class HabitationRequirement
    {
        public HabitationRequirementDefinition RequirementDefinition { get; private set; }

        public float Value { get; private set; }

        public float DecayTime { get; private set; }

        public void Decay()
        {
            Value -= DecayTime * Time.deltaTime;
        }
    }

    public enum HabitationRequirementDefinition
    {
        Water,
        Food,
        Tavern,
        Clothes,
        Pottery,
        Tool,
        Entertainment_1,
        Papyrus,
        Arts,
        Entertainment_2,
        Jewellery,
        Incense,
        Weapon
    }
}
