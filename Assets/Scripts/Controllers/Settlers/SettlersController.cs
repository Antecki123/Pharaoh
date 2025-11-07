using App.Helpers;
using App.Signals;
using Controllers.Work;
using Models.Economy;
using Models.Helpers;
using Models.Settler;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;
using Views.Settler;
using Zenject;

namespace Controllers.Settler
{
    public class SettlersController : IInitializable, ITickable
    {
        private List<(SettlerView, SettlerModel)> settlers = new List<(SettlerView, SettlerModel)>();

        private readonly SignalBus signalBus;
        private readonly HabitationModel habitationModel;
        private readonly EmploymentModel employmentModel;

        private readonly SettlerSpawner settlerSpawner;

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

            habitationModel.OnValueChanged += OnHabitationModelChanged;
            employmentModel.OnValueChanged += OnEmploymentModelChanged;
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
            newSettler.Item1.gameObject.SetActive(false);
            settlers.Add((newSettler.Item1, newSettler.Item2));

            var availableHabitat = habitationModel.GetAvailableHabitat();
            var availableEmployment = employmentModel.GetAvailableWorkplace();

            if (availableHabitat != null)
            {
                availableHabitat.AddResident(newSettler.Item2);
                newSettler.Item2.Habitation = availableHabitat ?? null;

                //newSettler.Item2.CurrentLocation = habitationModel.Habitations[availableHabitat];
            }

            if (availableEmployment != null)
            {
                availableEmployment.GetEmployer().AddWorker(newSettler.Item2);
                newSettler.Item2.Workplace = availableEmployment ?? null;

                newSettler.Item2.CurrentLocation = employmentModel.Workplaces[availableEmployment];
            }

            newSettler.Item1.InitAiStrategy();
        }

        private void DestroySettler(SettlersSignals.DespawnSettler signal)
        {
            var settlerToDespawn = settlers.FirstOrDefault(x => x.Item1 == signal.SettlerView);

            if (settlerToDespawn != default)
                settlers.Remove(settlerToDespawn);

            settlerToDespawn.Item2.Habitation.RemoveResident(settlerToDespawn.Item2);
        }

        private void OnHabitationModelChanged(CollectionChangeType changeType, HabitatModel habitation)
        {
            if (changeType == CollectionChangeType.Added)
            {
                foreach (var settler in settlers)
                {
                    if (settler.Item2.Habitation == null)
                    {
                        settler.Item2.Habitation = habitation;
                        habitation.AddResident(settler.Item2);
                    }
                }
            }
            else if (changeType == CollectionChangeType.Removed)
            {
                foreach (var resident in habitation.Residents)
                {
                    resident.Habitation = null;
                }
            }
        }

        private void OnEmploymentModelChanged(CollectionChangeType changeType, IWorkplace workplace)
        {
            if (changeType == CollectionChangeType.Added)
            {
                foreach(var settler in settlers)
                {
                    if (settler.Item2.Workplace == null)
                    {
                        settler.Item2.Workplace = workplace;
                        workplace.GetEmployer().AddWorker(settler.Item2);
                    }
                }
            }
            else if (changeType == CollectionChangeType.Removed)
            {
                foreach (var worker in workplace.GetEmployer().GetWorkers())
                {
                    if (worker is SettlerModel settler)
                        settler.Workplace = null;
                }
            }
        }
    }
}