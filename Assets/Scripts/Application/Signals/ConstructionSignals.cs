using Zenject;

namespace App.Signals
{
    public class ConstructionSignals
    {
        public ConstructionSignals(DiContainer container)
        {
            container.DeclareSignal<ConstructionMode>();
        }

        public class ConstructionMode
        {
            public bool State {  get; private set; }

            public ConstructionMode(bool state)
            {
                State = state;
            }
        }
    }
}