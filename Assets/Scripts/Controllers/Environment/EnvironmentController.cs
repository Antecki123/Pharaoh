using App.Configs;
using App.Signals;
using Models.Environment;
using System;
using UnityEngine;
using Zenject;

namespace Controllers.Environment
{
    public class EnvironmentController : IInitializable, ITickable
    {
        private readonly SignalBus signalBus;
        private readonly EnvironmentConfig environmentConfig;
        private readonly DateModel dateModel;

        private readonly IrrigationModel irrigationModel;

        [Header("River")]
        private float riverCurrentHeight;

        private float realTimeAccumulator;

        public EnvironmentController(SignalBus signalBus, EnvironmentConfig environmentConfig, DateModel dateModel, IrrigationModel irrigationModel)
        {
            this.signalBus = signalBus;
            this.environmentConfig = environmentConfig;
            this.dateModel = dateModel;
            this.irrigationModel = irrigationModel;
        }

        public void Initialize()
        {
            //signalBus.Fire(new EnvironmentSignals.DateChanged(currentMonth, currentYear));
        }

        public void Tick()
        {
            AdvanceMonth();
        }

        private void AdvanceMonth()
        {
            realTimeAccumulator += Time.deltaTime;
            if (realTimeAccumulator >= environmentConfig.MonthRealTimeDuration)
            {
                realTimeAccumulator -= environmentConfig.MonthRealTimeDuration;
                dateModel.CurrentMonth++;

                if (dateModel.CurrentMonth >= Enum.GetValues(typeof(MonthName)).Length)
                {
                    dateModel.CurrentMonth = 1;
                    dateModel.CurrentYear++;
                    if (dateModel.CurrentYear == 0)
                        dateModel.CurrentYear = 1;
                }

                //signalBus.Fire(new EnvironmentSignals.DateChanged(currentMonth, currentYear));
                CalculateRiverHeight();
            }
        }

        private void CalculateRiverHeight()
        {
            switch ((MonthName)dateModel.CurrentMonth)
            {
                case MonthName.Thoth:
                    riverCurrentHeight = UnityEngine.Random.Range(environmentConfig.RiverRiseMinHeight, environmentConfig.RiverRiseMaxHeight);
                    signalBus.Fire(new EnvironmentSignals.RiverSurfaceHeightChanged(riverCurrentHeight));
                    break;

                case MonthName.Khoiak:
                    riverCurrentHeight = UnityEngine.Random.Range(environmentConfig.RiverFallMinHeight, environmentConfig.RiverFallMaxHeight);
                    signalBus.Fire(new EnvironmentSignals.RiverSurfaceHeightChanged(riverCurrentHeight));
                    break;
            }
        }
    }
}