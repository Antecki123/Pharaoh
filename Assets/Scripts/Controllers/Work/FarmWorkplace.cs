using Cysharp.Threading.Tasks;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Work
{
    public class FarmWorkplace : IWorkplace
    {
        public List<CropModel> Crops { get; private set; } = new List<CropModel>();

        public float Range { get; private set; } = 25f;

        public Vector3 Position { get; private set; }

        public WorkplaceModel WorkplaceModel => workplaceModel;

        private WorkplaceModel workplaceModel;

        public FarmWorkplace(WorkplaceModel workplaceModel, Vector3 position)
        {
            this.workplaceModel = workplaceModel;
            Position = position;
        }

        public bool HasAvailableSpots()
        {
            return workplaceModel.MaxWorkersCount - workplaceModel.Workers.Count > 0;
        }

        public void Work()
        {
            foreach (var crop in Crops)
            {
                if (crop.IsWorkScheduled)
                    continue;

                switch (crop.CropFieldState)
                {
                    case CropFieldState.WaitingForPlanting:
                        _ = SchedulePlanting(crop);
                        break;
                    case CropFieldState.Growing:
                        break;
                    case CropFieldState.ReadyToHarvest:
                        _ = ScheduleHarvest(crop);
                        break;
                    default:
                        break;
                }
            }
        }

        private async UniTask SchedulePlanting(CropModel crop)
        {
            crop.IsWorkScheduled = true;
            crop.OnWorkScheduled?.Invoke("WheatFieldPlanting");
            crop.UpdateStatus(CropFieldState.Planting);

            await UniTask.WaitForSeconds(30);

            crop.UpdateStatus(CropFieldState.Growing);
            crop.IsWorkScheduled = false;
        }

        private async UniTask ScheduleHarvest(CropModel crop)
        {
            crop.IsWorkScheduled = true;
            crop.OnWorkScheduled?.Invoke("WheatFieldHarvesting");
            crop.UpdateStatus(CropFieldState.Harvesting);

            await UniTask.WaitForSeconds(20);

            crop.SetGrowthProgress(0f);
            crop.UpdateStatus(CropFieldState.WaitingForPlanting);
            crop.IsWorkScheduled = false;
        }
    }
}