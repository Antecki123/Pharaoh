using App.Helpers;
using App.Signals;
using Controllers.Ai;
using Controllers.Work;
using Models.Economy;
using Models.Habitation;
using Models.Helpers;
using Models.Settler;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Views.Settler;
using Zenject;

namespace Controllers.Settler
{
    public class SettlersController : IInitializable, ITickable, IDisposable
    {
        private List<SettlerPresenter> settlers = new List<SettlerPresenter>();

        private readonly SignalBus signalBus;
        private readonly HabitationModel habitationModel;
        private readonly EmploymentModel employmentModel;
        private readonly EconomyModel economyModel;

        private readonly SettlerSpawner settlerSpawner;
        private readonly Timer settlerSpawnTimer = new Timer(10f);

        private NativeArray<SettlerNeedsData> settlerNeedsArray;
        private NeedsUpdateJob needsUpdateJob;

        private TransformAccessArray settlerMovementArray;

        public SettlersController(SignalBus signalBus, PrefabManager prefabManager, HabitationModel habitationModel, EmploymentModel employmentModel, EconomyModel economyModel,
            SettlersNamesImporter settlersNames)
        {
            this.signalBus = signalBus;
            this.habitationModel = habitationModel;
            this.employmentModel = employmentModel;
            this.economyModel = economyModel;

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
            UpdateSettlersMovement();

            CheckSpawnSettler();
        }

        public void Dispose()
        {
            settlerNeedsArray.Dispose();
            settlerMovementArray.Dispose();
        }

        private void SpawnSettler(SettlersSignals.SpawnSettler signal)
        {
            var newSettler = settlerSpawner.SpawnSettler(signal.Position, signal.Rotation);
            newSettler.View.gameObject.SetActive(false);
            settlers.Add(new SettlerPresenter(newSettler.View, newSettler.Model));

            economyModel.AddSettlers(1);

            var availableHabitat = habitationModel.GetAvailableHabitat();
            var availableEmployment = employmentModel.GetAvailableWorkplace();

            if (availableHabitat != null)
            {
                availableHabitat.AddResident(newSettler.Model);
                newSettler.Model.Habitation = availableHabitat ?? null;

                newSettler.Model.CurrentLocation = habitationModel.Habitations[availableHabitat];
            }

            if (availableEmployment != null)
            {
                availableEmployment.GetEmployer().AddWorker(newSettler.Model);
                newSettler.Model.Workplace = availableEmployment ?? null;
            }
            newSettler.View.transform.position = habitationModel.Habitations[newSettler.Model.Habitation].transform.position;
            newSettler.View.InitAiStrategy();
        }

        private void DestroySettler(SettlersSignals.DespawnSettler signal)
        {
            var settlerToDespawn = settlers.FirstOrDefault(x => x.View == signal.SettlerView);

            if (settlerToDespawn != default)
                settlers.Remove(settlerToDespawn);

            settlerToDespawn.Model.Habitation.RemoveResident(settlerToDespawn.Model);

            economyModel.RemoveSettlers(1);
        }

        private void OnHabitationModelChanged(CollectionChangeType changeType, HabitatModel habitation)
        {
            if (changeType == CollectionChangeType.Added)
            {
                foreach (var settler in settlers)
                {
                    if (settler.Model.Habitation == null && habitation.HasAvailableSpot())
                    {
                        settler.Model.Habitation = habitation;
                        habitation.AddResident(settler.Model);
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
                    if (settler.Model.Workplace == null && workplace.GetEmployer().HasAvailableSpot())
                    {
                        settler.Model.Workplace = workplace;
                        workplace.GetEmployer().AddWorker(settler.Model);
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
                settler.View.Tick();
            }
        }

        private void UpdateSettlersNeeds()
        {
            settlerNeedsArray = new NativeArray<SettlerNeedsData>(settlers.Count, Allocator.Persistent);
            for (int i = 0; i < settlers.Count; i++)
            {
                var n = settlers[i].View.SettlerModel.SettlerNeeds;
                settlerNeedsArray[i] = new SettlerNeedsData()
                {
                    RestData = new SettlerNeedsData.NeedData(n.Rest.Value, n.Rest.DefaultDecayTime, n.Rest.RestoreFactor, n.Rest.IsRestoring),
                    EntertainmentData = new SettlerNeedsData.NeedData(n.Entertainment.Value, n.Entertainment.DefaultDecayTime, n.Entertainment.RestoreFactor, n.Entertainment.IsRestoring),
                    HealthData = new SettlerNeedsData.NeedData(n.Health.Value, n.Health.DefaultDecayTime, n.Health.RestoreFactor, n.Health.IsRestoring),
                    PrayData = new SettlerNeedsData.NeedData(n.Pray.Value, n.Pray.DefaultDecayTime, n.Pray.RestoreFactor, n.Pray.IsRestoring),
                    WorkData = new SettlerNeedsData.NeedData(n.Work.Value, n.Work.DefaultDecayTime, n.Work.RestoreFactor, n.Work.IsRestoring),
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
                settlers[i].View.SettlerModel.SettlerNeeds.Rest.Value = settlerNeedsArray[i].RestData.Value;
                settlers[i].View.SettlerModel.SettlerNeeds.Entertainment.Value = settlerNeedsArray[i].EntertainmentData.Value;
                settlers[i].View.SettlerModel.SettlerNeeds.Health.Value = settlerNeedsArray[i].HealthData.Value;
                settlers[i].View.SettlerModel.SettlerNeeds.Pray.Value = settlerNeedsArray[i].PrayData.Value;
                settlers[i].View.SettlerModel.SettlerNeeds.Work.Value = settlerNeedsArray[i].WorkData.Value;
            }
        }

        private void UpdateSettlersMovement()
        {
            var settlersToMove = new List<SettlerView>();
            for (int i = 0; i < settlers.Count; i++)
            {
                if (settlers[i].View.MovementHandler.RequiredMovement)
                    settlersToMove.Add(settlers[i].View);
            }

            var transforms = new Transform[settlersToMove.Count];
            var targetPositionsArray = new NativeArray<float3>(settlersToMove.Count, Allocator.TempJob);
            var movementSpeedsArray = new NativeArray<float>(settlersToMove.Count, Allocator.TempJob);

            for (int i = 0; i < settlersToMove.Count; i++)
            {
                transforms[i] = settlersToMove[i].transform;
                targetPositionsArray[i] = (float3)settlersToMove[i].MovementHandler.NextPosition;
                movementSpeedsArray[i] = settlersToMove[i].MovementHandler.MovementSpeed;
            }

            settlerMovementArray = new TransformAccessArray(transforms);
            var movementUpdateJob = new NpcMovementHandler.MovementJob()
            {
                targetPositions = targetPositionsArray,
                movementSpeeds = movementSpeedsArray,
                deltaTime = Time.deltaTime
            };

            var handle = movementUpdateJob.Schedule(settlerMovementArray);
            handle.Complete();

            foreach (var settler in settlersToMove)
            {
                if (Vector3.Distance(settler.transform.position, settler.MovementHandler.NextPosition) <= 0.1f)
                {
                    settler.MovementHandler.CurrentIndex++;

                    if (settler.MovementHandler.CurrentIndex >= settler.MovementHandler.Waypoints.Count)
                    {
                        settler.gameObject.SetActive(false);
                    }
                }
            }

            targetPositionsArray.Dispose();
            movementSpeedsArray.Dispose();
        }

        private void CheckSpawnSettler()
        {
            settlerSpawnTimer.Tick(Time.deltaTime);

            if (settlerSpawnTimer.IsFinished)
            {
                settlerSpawnTimer.Reset();
                if (habitationModel.Habitations.Any(x => x.Key.HasAvailableSpot()))
                {
                    SpawnSettler(new SettlersSignals.SpawnSettler(Vector3.zero, Quaternion.identity));
                }
            }
        }
    }

    public class SettlerPresenter
    {
        public SettlerView View { get; }
        public SettlerModel Model { get; }

        public SettlerPresenter(SettlerView view, SettlerModel model)
        {
            View = view;
            Model = model;
        }
    }
}