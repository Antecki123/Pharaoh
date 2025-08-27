using App.Signals;
using Models.Ai;
using UnityEngine;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionController : IInitializable, ITickable
    {
        private IConstruction currentConstruction;

        private SignalBus signalBus;
        private NavigationGraph navigationGraph;

        public ConstructionController(SignalBus signalBus, NavigationGraph navigationGraph)
        {
            this.signalBus = signalBus;
            this.navigationGraph = navigationGraph;
        }

        public void Initialize()
        {
            signalBus.Subscribe<ConstructionSignals.ConstructionMode>(SetConstruction);
        }

        public void Tick()
        {
            currentConstruction?.Tick();
        }

        private void SetConstruction(ConstructionSignals.ConstructionMode signal)
        {
            switch (signal.Building)
            {
                case BuildingDefinition.None:
                    currentConstruction = null;
                    break;
                case BuildingDefinition.Route:
                    currentConstruction = new RoadBuilder(signalBus, navigationGraph);
                    break;
                case BuildingDefinition.Cottage:
                    currentConstruction = new CottageBuilder();
                    break;
                case BuildingDefinition.House:
                    break;
                case BuildingDefinition.Residence:
                    break;
                default:
                    break;
            }
        }
    }

    public enum BuildingDefinition
    {
        None,
        Route,
        Cottage,
        House,
        Residence
    }
}
