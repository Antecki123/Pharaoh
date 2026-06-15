using Controllers.Work;
using Models.Helpers;
using System;
using System.Collections.Generic;
using Views.Construction;

namespace Models.Economy
{
    public class WorkplaceRepository
    {
        public event Action<IWorkplace, CollectionChangeType> OnValueChanged;

        private readonly Dictionary<BuildingView, IWorkplace> workplaces = new();

        public void AddWorkplace(BuildingView buildingView, IWorkplace workplace)
        {
            workplaces.Add(buildingView, workplace);
            OnValueChanged?.Invoke(workplace, CollectionChangeType.Added);
        }

        public void RemoveWorkplace(BuildingView buildingView)
        {
            var model = workplaces[buildingView];

            workplaces.Remove(buildingView);
            OnValueChanged?.Invoke(model, CollectionChangeType.Removed);
        }

        public IWorkplace GetWorkplace(BuildingView buildingView)
        {
            return workplaces[buildingView];
        }
    }
}