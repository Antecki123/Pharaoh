using Models.Economy;
using System;
using UnityEngine;

namespace Models.Work
{
    public class CropModel
    {
        public Action<string> OnWorkScheduled;

        public event Action OnValueChanged;

        public string Name { get; private set; }

        public CommodityModel ProducedCommodity { get; private set; }

        public CropFieldState CropFieldState { get; private set; }

        public Vector3 Position { get; private set; }

        public float GrowthDuration { get; private set; }

        public float GrowthProgress { get; private set; }

        public bool IsWorkScheduled { get; set; }

        public CropModel(string name, CommodityModel producedCommodity, Vector3 position, float growthDuration)
        {
            Name = name;
            ProducedCommodity = producedCommodity;
            Position = position;
            GrowthDuration = growthDuration;

            CropFieldState = CropFieldState.WaitingForPlanting;
        }

        public void CalcutateGrowth(float deltaTime)
        {
            if (CropFieldState == CropFieldState.Growing && GrowthProgress < 1f)
            {
                var progress = GrowthProgress + (deltaTime / GrowthDuration);
                SetGrowthProgress(progress);
                OnValueChanged?.Invoke();
            }

            if (CropFieldState == CropFieldState.Growing && GrowthProgress >= 1f)
            {
                UpdateStatus(CropFieldState.ReadyToHarvest);
                OnValueChanged?.Invoke();
            }
        }

        public void UpdateStatus(CropFieldState cropFieldState)
        {
            CropFieldState = cropFieldState;
            OnValueChanged?.Invoke();
        }

        public void SetGrowthProgress(float value)
        {
            GrowthProgress = value;
            OnValueChanged?.Invoke();
        }
    }

    public enum CropFieldState
    {
        WaitingForPlanting,
        Planting,
        Growing,
        ReadyToHarvest,
        Harvesting
    }
}