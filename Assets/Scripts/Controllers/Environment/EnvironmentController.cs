using App.Configs;
using App.Signals;
using System;
using UnityEngine;
using Zenject;

namespace Controllers.Environment
{
    public class EnvironmentController : IInitializable, ITickable
    {
        [Header("River")]
        private float riverCurrentHeight;

        [Header("Calendar")]
        private int currentMonth = 1;
        private int currentYear = 1;
        private float realTimeAccumulator;

        private readonly SignalBus signalBus;
        private readonly EnvironmentConfig environmentConfig;

        public EnvironmentController(SignalBus signalBus, EnvironmentConfig environmentConfig)
        {
            this.signalBus = signalBus;
            this.environmentConfig = environmentConfig;
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
                currentMonth++;

                if (currentMonth >= Enum.GetValues(typeof(MonthName)).Length)
                {
                    currentMonth = 1;
                    currentYear++;
                }

                //signalBus.Fire(new EnvironmentSignals.DateChanged(currentMonth, currentYear));
                CalculateRiverHeight();
            }
        }

        private void CalculateRiverHeight()
        {
            switch ((MonthName)currentMonth)
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

    public enum MonthName
    {
        Phamenoth = 1,
        Pharmuthi = 2,
        Pakhons = 3,
        Payni = 4,
        Epiphi = 5,
        Mesore = 6,
        Thoth = 7,
        Paopi = 8,
        Hathor = 9,
        Khoiak = 10,
        Tybi = 11,
        Mekhir = 12,

        Achet = Thoth | Paopi | Hathor | Khoiak
    }
}