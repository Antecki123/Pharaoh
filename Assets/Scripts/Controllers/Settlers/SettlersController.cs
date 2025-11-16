using App.Helpers;
using App.Signals;
using Controllers.Work;
using Models.Economy;
using Models.Helpers;
using Models.Settler;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Views.Settler;
using Zenject;

namespace Controllers.Settler
{
    public class SettlersController : IInitializable, ITickable, IDisposable
    {
        private List<(SettlerView, SettlerModel)> settlers = new List<(SettlerView, SettlerModel)>();
        private List<Transform> spawnPoints = new List<Transform>();

        private readonly SignalBus signalBus;
        private readonly HabitationModel habitationModel;
        private readonly EmploymentModel employmentModel;

        private readonly SettlerSpawner settlerSpawner;

        private NativeArray<SettlerNeedsData> settlerNeedsArray;
        private NeedsUpdateJob needsUpdateJob;

        //private float timer = 0;
        //private float timeSpan = 30f;

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
            SettlersTick();
            UpdateSettlersNeeds();

            /*timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = timeSpan;
                var spawnTransform = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                SpawnSettler(new SettlersSignals.SpawnSettler(spawnTransform.position, spawnTransform.rotation));
            }*/
        }

        public void Dispose()
        {
            settlerNeedsArray.Dispose();
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
                foreach (var settler in settlers)
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

        private void SettlersTick()
        {
            foreach (var settler in settlers)
            {
                settler.Item1.Tick();
            }
        }

        private void UpdateSettlersNeeds()
        {
            settlerNeedsArray = new NativeArray<SettlerNeedsData>(settlers.Count, Allocator.TempJob);
            for (int i = 0; i < settlers.Count; i++)
            {
                var n = settlers[i].Item1.SettlerModel.SettlerNeeds;
                settlerNeedsArray[i] = new SettlerNeedsData()
                {
                    RestData = new SettlerNeedsData.NeedData(n.Rest.Value, n.Rest.DefaultDecayTime, n.Rest.RestoreFactor, n.Rest.IsRestoring),
                    EntertainmentData = new SettlerNeedsData.NeedData(n.Entertainment.Value, n.Entertainment.DefaultDecayTime, n.Entertainment.RestoreFactor, n.Entertainment.IsRestoring),
                    HealthData = new SettlerNeedsData.NeedData(n.Health.Value, n.Health.DefaultDecayTime, n.Health.RestoreFactor, n.Health.IsRestoring),
                    PrayData = new SettlerNeedsData.NeedData(n.Pray.Value, n.Pray.DefaultDecayTime, n.Pray.RestoreFactor, n.Pray.IsRestoring),
                };
            }

            needsUpdateJob = new NeedsUpdateJob()
            {
                NeedsDataArray = settlerNeedsArray,
                DeltaTime = Time.deltaTime
            };

            var jobHandle = needsUpdateJob.Schedule(settlers.Count, 32);
            jobHandle.Complete();

            for (int i = 0; i < settlers.Count; i++)
            {
                settlers[i].Item1.SettlerModel.SettlerNeeds.Rest.Value = settlerNeedsArray[i].RestData.Value;
                settlers[i].Item1.SettlerModel.SettlerNeeds.Entertainment.Value = settlerNeedsArray[i].EntertainmentData.Value;
                settlers[i].Item1.SettlerModel.SettlerNeeds.Health.Value = settlerNeedsArray[i].HealthData.Value;
                settlers[i].Item1.SettlerModel.SettlerNeeds.Pray.Value = settlerNeedsArray[i].PrayData.Value;
            }
        }
    }
}