using Models.Helpers;
using System;
using System.Collections.Generic;
using Views.Construction;

namespace Models.Economy
{
    public class EmploymentRepository
    {
        public event Action<EmplyerModel, CollectionChangeType> OnValueChanged;

        private readonly Dictionary<BuildingView, EmplyerModel> employers = new();

        public void AddEmplyer(BuildingView buildingView, EmplyerModel emplyerModel)
        {
            employers.Add(buildingView, emplyerModel);
            OnValueChanged?.Invoke(emplyerModel, CollectionChangeType.Added);
        }

        public void RemoveEmplyer(BuildingView buildingView)
        {
            var model = employers[buildingView];

            employers.Remove(buildingView);
            OnValueChanged?.Invoke(model, CollectionChangeType.Removed);
        }

        public EmplyerModel GetAvailableEmployeeSlots()
        {
            foreach (var employer in employers)
            {
                if (employer.Value.GetAvailableEmployeeSlotsCount() > 0)
                    return employer.Value;
            }

            return null;
        }
    }
}