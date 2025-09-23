using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Work
{
    public class MaterialProcessingWorkplace : IWorkplace, ISupplyTarget
    {
        public List<object> currentWorkers = new List<object>();
        public float completionStatus;

        private CommodityModel requiredMaterial;
        private CommodityModel processedMaterial;

        private float processingTime;
        private int minWorkersCount;
        private int maxWorkersCount;

        public MaterialProcessingWorkplace(WorkplaceModel workplaceModel)
        {
            requiredMaterial = workplaceModel.RequiredMaterial;
            processedMaterial = workplaceModel.ProcessedMaterial;
            processingTime = workplaceModel.ProcessingTime;
            minWorkersCount = workplaceModel.MinimumWorkersCount;
            maxWorkersCount = workplaceModel.MaxWorkersCount;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetEntrancePosition()
        {
            throw new System.NotImplementedException();
        }

        public bool HasAvailableSpots()
        {
            return maxWorkersCount - currentWorkers.Count > 0;
        }

        public bool TryPickCommodity(CommodityModel commodity)
        {
            throw new System.NotImplementedException();
        }

        public void Work()
        {
            var efficiency = Mathf.Clamp01((float)currentWorkers.Count / maxWorkersCount);
            completionStatus += (Time.deltaTime / processingTime) * efficiency;

            if (completionStatus >= 1)
            {
                Debug.Log($"Processing <color=#ffffff>{processedMaterial.Name}</color> done.");

                //requiredMaterial.Quantity--;
                processedMaterial.Quantity++;
                completionStatus = 0;
            }
        }
    }
}