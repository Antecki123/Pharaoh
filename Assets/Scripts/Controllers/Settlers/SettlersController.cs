using App.Helpers;
using App.Signals;
using Models.Economy;
using Models.Settler;
using System.Collections.Generic;
using System.Linq;
using Views.Settler;
using Zenject;

namespace Controllers.Settler
{
    public class SettlersController : IInitializable, ITickable
    {
        private List<(SettlerView, SettlerModel)> settlers = new List<(SettlerView, SettlerModel)>();

        private SignalBus signalBus;
        private HabitationModel habitationModel;
        private EmploymentModel employmentModel;

        private SettlerSpawner settlerSpawner;

        public SettlersController(SignalBus signalBus, PrefabManager prefabManager, HabitationModel habitationModel, EmploymentModel employmentModel,
            SettlersNamesImporter settlersNames)
        {
            this.signalBus = signalBus;
            this.habitationModel = habitationModel;
            this.employmentModel = employmentModel;

            settlerSpawner = new SettlerSpawner(prefabManager, settlersNames);
        }

        public void Initialize()
        {
            signalBus.Subscribe<SettlersSignals.SpawnSettler>(SpawnSettler);
            signalBus.Subscribe<SettlersSignals.DespawnSettler>(DestroySettler);
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

            var habitat = habitationModel.Habitations.Keys.FirstOrDefault(x => x.HasAvailableSpots());
            newSettler.Item2.Habitation = habitat ?? null;
            habitat.AddResident(newSettler.Item1);
        }

        private void DestroySettler(SettlersSignals.DespawnSettler signal)
        {
            var settlerToDespawn = settlers.FirstOrDefault(x => x.Item1 == signal.SettlerView);

            if (settlerToDespawn != default)
                settlers.Remove(settlerToDespawn);

            settlerToDespawn.Item2.Habitation.RemoveResident(settlerToDespawn.Item1);
        }
    }
}