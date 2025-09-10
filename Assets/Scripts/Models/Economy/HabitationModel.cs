using System;
using System.Collections.Generic;
using Views.Settler;

namespace Models.Economy
{
    public interface IHabitation
    {
        public int GetMaxResidents();
        public IEnumerable<SettlerView> GetResidents();
    }

    public class HabitationModel
    {
        public event Action OnHabitationAdded;
        public event Action OnHabitationRemoved;

        public List<IHabitation> Habitations { get; set; } = new List<IHabitation>();


        public void AddHabitation(IHabitation habitation)
        {
            Habitations.Add(habitation);
            OnHabitationAdded?.Invoke();
        }

        public void RemoveHabitation(IHabitation habitation)
        {
            Habitations.Remove(habitation);
            OnHabitationRemoved?.Invoke();
        }
    }
}