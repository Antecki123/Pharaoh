using App.Helpers;
using App.Signals;
using Controllers.Ai;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class WorkersController : IInitializable, ITickable, IDisposable
    {
        private List<CarrierView> carriers = new List<CarrierView>();
        private List<ServiceAgentView> serviceAgents = new List<ServiceAgentView>();

        private readonly SignalBus signalBus;
        private readonly WorkerObjectPool<CarrierView> carriersObjectPool;
        private readonly WorkerObjectPool<ServiceAgentView> serviceAgentObjectPool;
        private readonly Transform workersContainer;

        private TransformAccessArray carriersMovementArray;
        private TransformAccessArray serviceAgentsMovementArray;

        public WorkersController(SignalBus signalBus, PrefabManager prefabManager)
        {
            this.signalBus = signalBus;

            workersContainer = new GameObject("WorkersContainer").transform;
            carriersObjectPool = new WorkerObjectPool<CarrierView>(prefabManager, workersContainer);
            serviceAgentObjectPool = new WorkerObjectPool<ServiceAgentView>(prefabManager, workersContainer);
        }

        public void Initialize()
        {
            signalBus.Subscribe<WorkplaceSignals.SpawnCarrier>(SpawnCarrier);
            signalBus.Subscribe<WorkplaceSignals.ReturnCarrier>(ReturnCarrier);
            signalBus.Subscribe<WorkplaceSignals.SpawnServiceAgent>(SpawnServiceAgent);
            signalBus.Subscribe<WorkplaceSignals.ReturnServiceAgent>(ReturnServiceAgent);
        }

        public void Tick()
        {
            WorkersTick();
            UpdateCarriersMovement();
            UpdateWorkersMovement();
        }

        public void Dispose()
        {
            signalBus.TryUnsubscribe<WorkplaceSignals.SpawnCarrier>(SpawnCarrier);
            signalBus.TryUnsubscribe<WorkplaceSignals.ReturnCarrier>(ReturnCarrier);
            signalBus.TryUnsubscribe<WorkplaceSignals.SpawnServiceAgent>(SpawnServiceAgent);
            signalBus.TryUnsubscribe<WorkplaceSignals.ReturnServiceAgent>(ReturnServiceAgent);

            carriersMovementArray.Dispose();
            serviceAgentsMovementArray.Dispose();
        }

        private void SpawnCarrier(WorkplaceSignals.SpawnCarrier signal)
        {
            carriersObjectPool.WorkersPool.Get(out CarrierView carrier);
            carrier.Init(signal.CarrierTasks);
            carrier.OnTasksFinished += signal.OnTasksFinished;

            carriers.Add(carrier);
        }

        private void ReturnCarrier(WorkplaceSignals.ReturnCarrier signal)
        {
            carriersObjectPool.WorkersPool.Release(signal.Carrier);
            carriers.Remove(signal.Carrier);
        }

        private void SpawnServiceAgent(WorkplaceSignals.SpawnServiceAgent signal)
        {
            serviceAgentObjectPool.WorkersPool.Get(out ServiceAgentView agent);
            agent.Init(signal.Origin);
            agent.OnAgentReturn += signal.OnAgentReturn;

            serviceAgents.Add(agent);
        }

        private void ReturnServiceAgent(WorkplaceSignals.ReturnServiceAgent signal)
        {
            serviceAgentObjectPool.WorkersPool.Release(signal.Agent);
            serviceAgents.Remove(signal.Agent);
        }

        private void WorkersTick()
        {
            foreach (var carrier in carriers)
                carrier.Tick();

            foreach (var agent in serviceAgents)
                agent.Tick();
        }

        private void UpdateCarriersMovement()
        {
            var carriersToMove = new List<CarrierView>();
            for (int i = 0; i < carriers.Count; i++)
            {
                if (carriers[i].MovementHandler.RequiredMovement)
                    carriersToMove.Add(carriers[i]);
            }

            var transforms = new Transform[carriersToMove.Count];
            var targetPositionsArray = new NativeArray<float3>(carriersToMove.Count, Allocator.TempJob);
            var movementSpeedsArray = new NativeArray<float>(carriersToMove.Count, Allocator.TempJob);

            for (int i = 0; i < carriersToMove.Count; i++)
            {
                transforms[i] = carriersToMove[i].transform;
                targetPositionsArray[i] = (float3)carriersToMove[i].MovementHandler.NextPosition;
                movementSpeedsArray[i] = carriersToMove[i].MovementHandler.MovementSpeed;
            }

            carriersMovementArray = new TransformAccessArray(transforms);
            var movementUpdateJob = new NpcMovementHandler.MovementJob()
            {
                targetPositions = targetPositionsArray,
                movementSpeeds = movementSpeedsArray,
                deltaTime = Time.deltaTime
            };

            var handle = movementUpdateJob.Schedule(carriersMovementArray);
            handle.Complete();

            var waypointReachedDistance = 0.2f;

            foreach (var carrier in carriersToMove)
            {
                var distanceToTargetSqrt = (carrier.transform.position - carrier.MovementHandler.NextPosition).sqrMagnitude;
                if (distanceToTargetSqrt <= waypointReachedDistance * waypointReachedDistance)
                {
                    carrier.MovementHandler.CurrentIndex++;
                    if (carrier.MovementHandler.CurrentIndex >= carrier.MovementHandler.Waypoints.Count)
                    {
                        carrier.FinishTask();
                    }
                }
            }

            targetPositionsArray.Dispose();
            movementSpeedsArray.Dispose();
        }

        private void UpdateWorkersMovement()
        {
            var agentsToMove = new List<ServiceAgentView>();
            for (int i = 0; i < serviceAgents.Count; i++)
            {
                if (serviceAgents[i].MovementHandler.RequiredMovement)
                    agentsToMove.Add(serviceAgents[i]);
            }

            var transforms = new Transform[agentsToMove.Count];
            var targetPositionsArray = new NativeArray<float3>(agentsToMove.Count, Allocator.TempJob);
            var movementSpeedsArray = new NativeArray<float>(agentsToMove.Count, Allocator.TempJob);

            for (int i = 0; i < agentsToMove.Count; i++)
            {
                transforms[i] = agentsToMove[i].transform;
                targetPositionsArray[i] = (float3)agentsToMove[i].MovementHandler.NextPosition;
                movementSpeedsArray[i] = agentsToMove[i].MovementHandler.MovementSpeed;
            }

            serviceAgentsMovementArray = new TransformAccessArray(transforms);
            var movementUpdateJob = new NpcMovementHandler.MovementJob()
            {
                targetPositions = targetPositionsArray,
                movementSpeeds = movementSpeedsArray,
                deltaTime = Time.deltaTime
            };

            var handle = movementUpdateJob.Schedule(serviceAgentsMovementArray);
            handle.Complete();

            var waypointReachedDistance = 0.2f;

            foreach (var agent in agentsToMove)
            {
                if (agent.IsReturning)
                {
                    var distanceToNextPositionSqrt = (agent.transform.position - agent.MovementHandler.NextPosition).sqrMagnitude;
                    if (distanceToNextPositionSqrt <= waypointReachedDistance * waypointReachedDistance)
                        agent.MovementHandler.CurrentIndex++;

                    var distanceToTargetSqrt = (agent.transform.position - agent.MovementHandler.TargetPosition).sqrMagnitude;
                    if (distanceToTargetSqrt <= waypointReachedDistance * waypointReachedDistance)
                        agent.FinishWork();
                }
                else
                {
                    var distanceToTarget = (agent.transform.position - agent.MovementHandler.NextPosition).sqrMagnitude;
                    if (distanceToTarget <= 0.25f)
                    {
                        if (agent.RemainingCapacity < 0)
                            agent.ReturnToOrigin();
                        else
                            agent.CalculateNextPosition();
                    }
                }
            }

            targetPositionsArray.Dispose();
            movementSpeedsArray.Dispose();
        }
    }

    public class WorkerObjectPool<T> where T : MonoBehaviour
    {
        public ObjectPool<T> WorkersPool => workersPool;

        private readonly ObjectPool<T> workersPool;
        private readonly Transform workersContainer;

        private readonly PrefabManager prefabManager;

        public WorkerObjectPool(PrefabManager prefabManager, Transform workersContainer, bool collectionCheck = true,
            int defaultCapacity = 30, int maxSize = 500)
        {
            this.prefabManager = prefabManager;
            this.workersContainer = workersContainer;

            workersPool = new ObjectPool<T>(
            createFunc: Create,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroy,
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize);
        }

        private T Create()
        {
            var worker = prefabManager.Instantiate<T>(typeof(T).Name);
            worker.transform.SetParent(workersContainer);

            return worker;
        }

        private void OnGet(T worker)
        {
            worker.gameObject.SetActive(true);
        }

        private void OnRelease(T worker)
        {
            worker.gameObject.SetActive(false);
        }

        private void OnDestroy(T worker)
        {
            UnityEngine.Object.Destroy(worker.gameObject);
        }
    }
}