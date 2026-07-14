using Controllers.Work;
using Models.Economy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Work
{
    public class DistributionPointModel : IWorkplace
    {
        public event Action<DistributionPointModel> OnValueChanged;

        public bool IsRunning => CurrentWorkersCount > WorkplaceDefinition.MinimumWorkersCount;

        public DistributionWorkplaceDefinition WorkplaceDefinition { get; private set; }

        public float ProcessingProgress { get; private set; } = 0;

        public int CurrentWorkersCount { get; private set; } = 0;

        public bool IsCarrierAvailable { get; set; } = true;

        public bool IsServiceAgentAvailable { get; set; } = true;

        public HashSet<Vector2Int> InfluencedTiles = new();

        public DistributionPointModel(DistributionWorkplaceDefinition workplaceDefinition)
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

        public void UseServiceAgent()
        {
            IsServiceAgentAvailable = false;
            OnValueChanged?.Invoke(this);
        }

        public void ReturnServiceAgent()
        {
            IsServiceAgentAvailable = true;
            OnValueChanged?.Invoke(this);
        }

        public void SetProcessingProgress(float value)
        {
            ProcessingProgress = value;
            OnValueChanged?.Invoke(this);
        }
    }

    public struct DistributionWorkplaceDefinition
    {
        public string Name { get; set; }

        public CommodityModel RequiredCommodity { get; set; }

        public List<IService> Services { get; set; }

        public float ProcessingTime { get; set; }

        public int MinimumWorkersCount { get; set; }

        public int MaxWorkersCount { get; set; }

        public float Range { get; set; }
    }
}