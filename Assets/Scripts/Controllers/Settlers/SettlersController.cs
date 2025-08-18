using Models.Ai.Pathfinding;
using Models.Settler;
using System.Collections.Generic;
using UnityEngine;
using Views.Settler;
using Zenject;

namespace Controllers.Settler
{
    public class SettlersController : IInitializable, ITickable
    {
        [Inject] private SignalBus signalBus;

        private Dictionary<SettlerView, SettlerModel> settlers = new Dictionary<SettlerView, SettlerModel>();
        private IPathfindingBrain<Vector2> pathfinding;

        private SettlerSpawner settlerSpawner;

        public void Initialize()
        {
            //signalBus.Subscribe<object>(SpawnSettler);
            //pathfinding = new DStarLite<Vector2>();
            //pathfinding.Initialize(new List<Node<Vector2>>());

            settlerSpawner = new SettlerSpawner();
            //SpawnSettler();
        }

        public void Tick()
        {
            foreach (var settler in settlers)
            {
                settler.Key.Tick();
            }
        }

        private void SpawnSettler()
        {
            var newSettler = settlerSpawner.SpawnSettler(Vector3.zero);
            settlers.Add(newSettler.View, newSettler.Model);
        }
    }
}