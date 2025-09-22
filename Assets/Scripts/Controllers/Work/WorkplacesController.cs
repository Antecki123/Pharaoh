using App.Signals;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Controllers.Work
{
    public interface IWorkplace
    {
        public bool HasAvailableSpots();
        public void Work();
    }

    public class WorkplacesController : IInitializable, ITickable
    {
        public List<IWorkplace> workplaces = new List<IWorkplace>();
        public List<CropModel> cropFields = new List<CropModel>();

        private SignalBus signalBus;

        public WorkplacesController(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public void Initialize()
        {
            signalBus.Subscribe<WorkplaceSignals.RegisterWorkplace>(RegisterWorkplace);
            signalBus.Subscribe<WorkplaceSignals.UnregisterWorkplace>(UnregisterWorkplace);
            signalBus.Subscribe<WorkplaceSignals.RegisterCropField>(RegisterCropField);
            signalBus.Subscribe<WorkplaceSignals.UnregisterCropField>(UnregisterCropField);
        }

        public void Tick()
        {
            foreach (var workplace in workplaces)
            {
                workplace?.Work();
            }

            foreach (var crop in cropFields)
            {
                crop?.CalcutateGrowth(Time.deltaTime);
            }
        }

        public void RegisterWorkplace(WorkplaceSignals.RegisterWorkplace signal)
        {
            workplaces.Add(signal.Workplace);

            if (signal.Workplace is FarmWorkplace)
                AssignFieldsToFarms();
        }

        public void UnregisterWorkplace(WorkplaceSignals.UnregisterWorkplace signal)
        {
            workplaces.Remove(signal.Workplace);

            if (signal.Workplace is FarmWorkplace)
                AssignFieldsToFarms();
        }

        public void RegisterCropField(WorkplaceSignals.RegisterCropField signal)
        {
            cropFields.Add(signal.CropModel);
            AssignFieldsToFarms();
        }

        public void UnregisterCropField(WorkplaceSignals.UnregisterCropField signal)
        {
            cropFields.Remove(signal.CropModel);
            AssignFieldsToFarms();
        }

        private void AssignFieldsToFarms()
        {
            var farms = new List<FarmWorkplace>();
            foreach (var workplace in workplaces)
            {
                if (workplace is FarmWorkplace)
                    farms.Add(workplace as FarmWorkplace);
            }

            foreach (var farm in farms)
                farm.Crops.Clear();

            foreach (var field in cropFields)
            {
                FarmWorkplace nearestFarm = null;
                float nearestDist = float.MaxValue;

                foreach (var farm in farms)
                {
                    float dist = Vector3.Distance(farm.Position, field.Position);

                    if (dist <= farm.Range && dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestFarm = farm;
                    }
                }

                nearestFarm?.Crops.Add(field);
            }
        }
    }
}