using Controllers.Work;
using Models.Economy;
using System;

namespace Models.Work
{
    public class FarmWorkplaceModel : IWorkplace
    {
        public event Action<FarmWorkplaceModel> OnValueChanged;

        public FarmWorkplaceDefinition WorkplaceDefinition { get; private set; }

        public float ProcessingProgress { get; private set; } = 0;

        public int CurrentWorkersCount { get; private set; } = 0;

        public bool IsCarrierAvailable { get; set; } = true;

        public FarmWorkplaceModel(FarmWorkplaceDefinition workplaceDefinition)
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

    public struct FarmWorkplaceDefinition
    {
        public string Name { get; set; }

        public CommodityModel CreatedCommodity { get; set; }

        public float ProcessingTime { get; set; }

        public int MinimumWorkersCount { get; set; }

        public int MaxWorkersCount { get; set; }
    }
}