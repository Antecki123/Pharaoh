using Controllers.Work;
using Models.Economy;
using System;

namespace Models.Work
{
    public class WorkplaceModel : IWorkplace
    {
        public event Action<WorkplaceModel> OnValueChanged;

        public bool IsRunning => CurrentWorkersCount > WorkplaceDefinition.MinimumWorkersCount;

        public WorkplaceDefinition WorkplaceDefinition { get; private set; }

        public float ProcessingProgress { get; private set; } = 0;

        public int CurrentWorkersCount { get; private set; } = 0;

        public bool IsCarrierAvailable { get; set; } = true;

        public WorkplaceModel(WorkplaceDefinition workplaceDefinition)
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

        public void UseCarrier()
        {
            IsCarrierAvailable = false;
            OnValueChanged?.Invoke(this);
        }

        public void ReturnCarrier()
        {
            IsCarrierAvailable = true;
            OnValueChanged?.Invoke(this);
        }

        public void SetProcessingProgress(float value)
        {
            ProcessingProgress = value;
            OnValueChanged?.Invoke(this);
        }
    }

    public struct WorkplaceDefinition
    {
        public string Name { get; set; }

        public CommodityModel RequiredCommodity { get; set; }

        public CommodityModel ProcessedCommodity { get; set; }

        public float ProcessingTime { get; set; }

        public int MinimumWorkersCount { get; set; }

        public int MaxWorkersCount { get; set; }
    }
}