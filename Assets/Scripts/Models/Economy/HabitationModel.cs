using System;
using System.Collections.Generic;
using Views.Construction;
using Views.Settler;

namespace Models.Economy
{
    public class HabitationModel
    {
        public IReadOnlyDictionary<HabitatModel, BuildingView> Habitations => habitations;

        private Dictionary<HabitatModel, BuildingView> habitations = new Dictionary<HabitatModel, BuildingView>();

        public void AddHabitation(HabitatModel habitation, BuildingView buildingView)
        {
            habitations.Add(habitation, buildingView);
        }

        public void RemoveHabitation(HabitatModel habitation)
        {
            habitations.Remove(habitation);
        }
    }

    public class HabitatModel
    {
        public event Action OnValueChanged;

        public string Name { get; private set; }

        public int MaxResidents { get; private set; }

        public IReadOnlyList<SettlerView> Residents => residents;

        public IReadOnlyList<CommodityModel> Storage => storage;

        private List<SettlerView> residents = new List<SettlerView>();
        private List<CommodityModel> storage = new List<CommodityModel>();

        public HabitatModel(string name, int maxResidents)
        {
            Name = name;
            MaxResidents = maxResidents;
        }

        public void AddResident(SettlerView settler)
        {
            residents.Add(settler);
            OnValueChanged?.Invoke();
        }

        public void RemoveResident(SettlerView settler)
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

        public bool HasAvailableSpots()
        {
            return MaxResidents - residents.Count > 0;
        }
    }
}