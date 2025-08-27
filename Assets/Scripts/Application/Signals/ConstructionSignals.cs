using Controllers.Construction;
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
            public BuildingDefinition Building {  get; private set; }

            public ConstructionMode(BuildingDefinition building)
            {
                Building = building;
            }
        }
    }
}