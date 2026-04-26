using App.Helpers;
using App.Signals;
using Controllers.Ai;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public interface IWorker
    {
        public void Tick();
    }

    public class WorkersController : IInitializable, ITickable, IDisposable
    {
        // TODO: remove carriers and serviceAgents collections
        private List<CarrierView> carriers = new List<CarrierView>();
        private List<ServiceAgentView> serviceAgents = new List<ServiceAgentView>();
        private Dictionary<IWorker, IWorkplace> workersWorkplaceMap = new Dictionary<IWorker, IWorkplace>();

        private readonly SignalBus signalBus;
        private readonly WorkerObjectPool<CarrierView> carriersObjectPool;
        private readonly WorkerObjectPool<ServiceAgentView> serviceAgentObjectPool;
        private readonly Transform workersContainer;

        private TransformAccessArray carriersMovementArray;
        private TransformAccessArray serviceAgentsMovementArray;

        private const float waypointReachedDistance = 0.2f;

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
            signalBus.Subscribe<WorkplaceSignals.WorklplaceDestroyed>(WorklplaceDestroyed);
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
            signalBus.TryUnsubscribe<WorkplaceSignals.WorklplaceDestroyed>(WorklplaceDestroyed);

            carriersMovementArray.Dispose();
            serviceAgentsMovementArray.Dispose();
        }

        private void SpawnCarrier(WorkplaceSignals.SpawnCarrier signal)
        {
            carriersObjectPool.WorkersPool.Get(out CarrierView carrier);
            carrier.Init(signal.CarrierTasks);
            carrier.OnTasksFinished += signal.OnTasksFinished;

            carriers.Add(carrier);
            workersWorkplaceMap.Add(carrier, signal.Workplace);
        }

        private void ReturnCarrier(WorkplaceSignals.ReturnCarrier signal)
        {
            carriersObjectPool.WorkersPool.Release(signal.Carrier);

            carriers.Remove(signal.Carrier);
            workersWorkplaceMap.Remove(signal.Carrier);
        }

        private void SpawnServiceAgent(WorkplaceSignals.SpawnServiceAgent signal)
        {
            serviceAgentObjectPool.WorkersPool.Get(out ServiceAgentView agent);
            agent.Init(signal.ServiceAgentPayload);
            agent.OnAgentReturn += signal.OnAgentReturn;

            serviceAgents.Add(agent);
            workersWorkplaceMap.Add(agent, signal.Workplace);
        }

        private void ReturnServiceAgent(WorkplaceSignals.ReturnServiceAgent signal)
        {
            serviceAgentObjectPool.WorkersPool.Release(signal.Agent);

            serviceAgents.Remove(signal.Agent);
            workersWorkplaceMap.Remove(signal.Agent);
        }

        private void WorklplaceDestroyed(WorkplaceSignals.WorklplaceDestroyed signal)
        {
            var toRemove = new List<IWorker>();

            foreach (var kv in workersWorkplaceMap)
            {
                if (kv.Value != signal.Workplace)
                    continue;

                var worker = kv.Key;

                switch (worker)
                {
                    case CarrierView carrier:
                        carriersObjectPool.WorkersPool.Release(carrier);
                        carriers.Remove(carrier);
                        break;

                    case ServiceAgentView agent:
                        serviceAgentObjectPool.WorkersPool.Release(agent);
                        serviceAgents.Remove(agent);
                        break;
                }

                toRemove.Add(worker);
            }

            foreach (var worker in toRemove)
            {
                workersWorkplaceMap.Remove(worker);
            }
        }

        private void WorkersTick()
        {
            foreach (var worker in workersWorkplaceMap)
            {
                worker.Key.Tick();
            }
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
}