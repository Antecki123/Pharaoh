using Controllers.Work;
using System;

namespace Models.Economy
{
    public class StorageWorkplaceModel : IWorkplace
    {
        public event Action<StorageWorkplaceModel> OnValueChanged;

        public bool IsRunning => CurrentWorkersCount > WorkplaceDefinition.MinimumWorkersCount;

        public StorageWorkplaceDefinition WorkplaceDefinition { get; private set; }

        public int CurrentWorkersCount { get; private set; } = 0;

        public StorageWorkplaceModel(StorageWorkplaceDefinition workplaceDefinition)
        {
            WorkplaceDefinition = workplaceDefinition;
        }

        public void AddWorker()
        {
            CurrentWorkersCount++;
            OnValueChanged?.Invoke(this);
        }

        public void RemoveWorker()
        {
            CurrentWorkersCount--;
            OnValueChanged?.Invoke(this);
        }
    }

    public struct StorageWorkplaceDefinition
    {
        public string Name { get; set; }

        public int MinimumWorkersCount { get; set; }

        public int MaxWorkersCount { get; set; }
    }
}