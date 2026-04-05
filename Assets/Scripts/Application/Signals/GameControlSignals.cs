using Zenject;

namespace App.Signals
{
    public class GameControlSignals
    {
        public GameControlSignals(DiContainer container)
        {
            container.DeclareSignal<GameSpeed>();
            container.DeclareSignal<ScenarioStarted>();
            container.DeclareSignal<ScenarioFinished>();
            container.DeclareSignal<MissionCompleted>();
            container.DeclareSignal<MissionFailed>();
            container.DeclareSignal<OpenPauseMenu>();
        }

        public class GameSpeed
        {
            public float Speed { get; private set; }

            public GameSpeed(float speed)
            {
                Speed = speed;
            }
        }

        public class ScenarioStarted { }

        public class ScenarioFinished { }

        public class MissionCompleted { }

        public class MissionFailed { }

        public class OpenPauseMenu
        {
            public bool State { get; private set; }

            public OpenPauseMenu(bool state)
            {
                State = state;
            }
        }
    }
}