using Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Views.Construction;

namespace Models.Economy
{
    public class HabitationModel
    {
        public event Action<CollectionChangeType, HabitatModel> OnValueChanged;

        public IReadOnlyDictionary<HabitatModel, BuildingView> Habitations => habitations;

        private readonly Dictionary<HabitatModel, BuildingView> habitations = new Dictionary<HabitatModel, BuildingView>();

        public void AddHabitation(HabitatModel habitation, BuildingView buildingView)
        {
            habitations.Add(habitation, buildingView);
            OnValueChanged?.Invoke(CollectionChangeType.Added, habitation);
        }

        public void RemoveHabitation(HabitatModel habitation)
        {
            habitations.Remove(habitation);
            OnValueChanged?.Invoke(CollectionChangeType.Removed, habitation);
        }

        public HabitatModel GetAvailableHabitat()
        {
            return habitations.Keys
                .OrderBy(x => x.Residents.Count)
                .Where(x => x.HasAvailableSpot())
                .FirstOrDefault();
        }

        public int GetHousingCapacity()
        {
            var capacity = 0;
            foreach (var habitat in habitations)
                capacity += habitat.Key.MaxResidents;

            return capacity;
        }
    }
}