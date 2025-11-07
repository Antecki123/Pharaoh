using Zenject;

namespace App.Signals
{
    public class EnvironmentSignals
    {
        public EnvironmentSignals(DiContainer container)
        {
            container.DeclareSignal<RiverSurfaceHeightChanged>();
            container.DeclareSignal<DateChanged>();
        }

        public class RiverSurfaceHeightChanged
        {
            public float RiverSurfaceHeight { get; private set; }

            public RiverSurfaceHeightChanged(float riverSurfaceHeight)
            {
                RiverSurfaceHeight = riverSurfaceHeight;
            }
        }

        public class DateChanged
        {
            public int Month { get; private set; }

            public int Year { get; private set; }

            public DateChanged(int month, int year)
            {
                Month = month;
                Year = year;
            }
        }
    }
}