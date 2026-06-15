using Models.Helpers;
using Models.Settler;
using System;
using System.Collections.Generic;
using Views.Construction;

namespace Models.Economy
{
    public class EmplyerModel
    {
        public event Action<CollectionChangeType> OnValueChanged;

        public HashSet<SettlerModel> CurrentEmployees => currentEmployees;
        public BuildingView BuildingView => buildingView;

        private readonly HashSet<SettlerModel> currentEmployees = new();

        private readonly BuildingView buildingView;
        private readonly int maxEmplyerCount;

        public EmplyerModel(BuildingView buildingView, int maxEmplyerCount)
        {
            this.buildingView = buildingView;
            this.maxEmplyerCount = maxEmplyerCount;
        }

        public void AddEmployee(SettlerModel settler)
        {
            if (currentEmployees.Count >= maxEmplyerCount)
                return;

            currentEmployees.Add(settler);
            OnValueChanged?.Invoke(CollectionChangeType.Added);
        }

        public void RemoveEmployee(SettlerModel settler)
        {
            if (currentEmployees.Count <= 0)
                return;

            currentEmployees.Remove(settler);
            OnValueChanged?.Invoke(CollectionChangeType.Removed);
        }

        public int GetAvailableEmployeeSlotsCount()
        {
            return Math.Max(0, maxEmplyerCount - currentEmployees.Count);
        }
    }
}