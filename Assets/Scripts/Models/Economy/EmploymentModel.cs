using Controllers.Work;
using Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Views.Construction;

namespace Models.Economy
{
    public interface IEmployer
    {
        public ICollection<IEmployee> GetWorkers();
        public void AddWorker(IEmployee worker);
        public void RemoveWorker(IEmployee worker);
        public bool HasAvailableSpot();
    }

    public interface IEmployee { }

    public class EmploymentModel
    {
        public event Action<CollectionChangeType, IWorkplace> OnValueChanged;

        public IReadOnlyDictionary<IWorkplace, BuildingView> Workplaces => workplaces;

        private Dictionary<IWorkplace, BuildingView> workplaces = new Dictionary<IWorkplace, BuildingView>();

        public void AddWorkplace(IWorkplace workplace, BuildingView buildingView)
        {
            workplaces.Add(workplace, buildingView);
            OnValueChanged?.Invoke(CollectionChangeType.Added, workplace);
        }

        public void RemoveWorkplace(IWorkplace workplace)
        {
            workplaces.Remove(workplace);
            OnValueChanged?.Invoke(CollectionChangeType.Removed, workplace);
        }

        public IWorkplace GetAvailableWorkplace()
        {
            return workplaces.Keys
                .Where(x => x.GetEmployer().HasAvailableSpot())
                .FirstOrDefault();
        }
    }
}