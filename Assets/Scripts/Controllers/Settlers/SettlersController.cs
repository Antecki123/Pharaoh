using App.Helpers;
using App.Signals;
using Models.Ai;
using Models.Economy;
using Models.Settler;
using System.Collections.Generic;
using UnityEngine;
using Views.Settler;
using Zenject;

namespace Controllers.Settler
{
    public class SettlersController : IInitializable, ITickable
    {
        private List<(SettlerView, SettlerModel)> settlers = new List<(SettlerView, SettlerModel)>();

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private NavigationGraph navigationGraph;
        private HabitationModel habitationModel;

        private SettlerSpawner settlerSpawner;

        public SettlersController(SignalBus signalBus, PrefabManager prefabManager, NavigationGraph navigationGraph, HabitationModel habitationModel)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.navigationGraph = navigationGraph;
            this.habitationModel = habitationModel;

            settlerSpawner = new SettlerSpawner(prefabManager);
        }

        public void Initialize()
        {
            habitationModel.OnHabitationAdded += () => signalBus.Fire(new SettlersSignals.SpawnSettler(new Vector3(40, 0, 0), Quaternion.identity));

            signalBus.Subscribe<SettlersSignals.SpawnSettler>(SpawnSettler);
        }

        public void Tick()
        {
            foreach (var settler in settlers)
            {
                settler.Item1.Tick();
            }
        }

        private void SpawnSettler(SettlersSignals.SpawnSettler signal)
        {
            var newSettler = settlerSpawner.SpawnSettler(signal.Position, signal.Rotation);
            settlers.Add((newSettler.Item1, newSettler.Item2));
        }
    }
}