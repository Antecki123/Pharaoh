using Controllers.Construction;
using Zenject;

namespace App.Signals
{
    public class ConstructionSignals
    {
        public ConstructionSignals(DiContainer container)
        {
            container.DeclareSignal<ConstructionMode>();
            container.DeclareSignal<DestroyMode>();
            container.DeclareSignal<ActivateConstructionMode>();
        }

        public class ConstructionMode
        {
            public BuildingDefinition Building { get; private set; }

            public ConstructionMode(BuildingDefinition building)
            {
                Building = building;
            }
        }

        public class DestroyMode
        {

        }

        public class ActivateConstructionMode
        {
            public bool State { get; private set; }

            public ActivateConstructionMode(bool state)
            {
                State = state;
            }
        }
    }
}