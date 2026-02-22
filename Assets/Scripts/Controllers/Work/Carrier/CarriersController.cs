using App.Helpers;
using App.Signals;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using Views.Settler;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class CarriersController : IInitializable, ITickable, IDisposable
    {
        private List<CarrierView> carriers = new List<CarrierView>();

        private readonly SignalBus signalBus;
        private readonly CarriersObjectPool carriersObjectPool;

        private TransformAccessArray carriersMovementArray;

        public CarriersController(SignalBus signalBus, PrefabManager prefabManager)
        {
            this.signalBus = signalBus;
            carriersObjectPool = new CarriersObjectPool(prefabManager);
        }

        public void Initialize()
        {
            signalBus.Subscribe<WorkplaceSignals.SpawnCarrier>(SpawnCarrier);
            signalBus.Subscribe<WorkplaceSignals.ReturnCarrier>(ReturnCarrier);
        }

        public void Tick()
        {
            CarriersTick();
            UpdateCarriersMovement();
        }

        public void Dispose()
        {
            signalBus.TryUnsubscribe<WorkplaceSignals.SpawnCarrier>(SpawnCarrier);
            signalBus.TryUnsubscribe<WorkplaceSignals.ReturnCarrier>(ReturnCarrier);

            carriersMovementArray.Dispose();
        }

        private void SpawnCarrier(WorkplaceSignals.SpawnCarrier signal)
        {
            carriersObjectPool.CarriersPool.Get(out CarrierView carrier);
            carrier.Init(signal.CarrierTasks);
            carrier.OnTasksFinished += signal.OnTasksFinished;

            carriers.Add(carrier);
        }

        private void ReturnCarrier(WorkplaceSignals.ReturnCarrier signal)
        {
            carriersObjectPool.CarriersPool.Release(signal.Carrier);
            carriers.Remove(signal.Carrier);
        }

        private void CarriersTick()
        {
            foreach (var carrier in carriers)
            {
                carrier.Tick();
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
                movementSpeedsArray[i] = carriersToMove[i].MovementSpeed;
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
                var distanceToTarget = (carrier.transform.position - carrier.MovementHandler.NextPosition).sqrMagnitude;
                if (distanceToTarget <= 0.1f)
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
    }

    public class CarriersObjectPool
    {
        public ObjectPool<CarrierView> CarriersPool => carriersPool;

        private ObjectPool<CarrierView> carriersPool;
        private Transform carriersContainer;

        private readonly PrefabManager prefabManager;

        public CarriersObjectPool(PrefabManager prefabManager)
        {
            this.prefabManager = prefabManager;

            carriersContainer = new GameObject("CarriersContainer").transform;

            carriersPool = new ObjectPool<CarrierView>(
            createFunc: Create,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroy,
            collectionCheck: true,
            defaultCapacity: 30,
            maxSize: 500);
        }

        private CarrierView Create()
        {
            var carrier = prefabManager.Instantiate<CarrierView>("CarrierView");
            carrier.transform.SetParent(carriersContainer);

            return carrier;
        }

        private void OnGet(CarrierView carrier)
        {
            carrier.gameObject.SetActive(true);
        }

        private void OnRelease(CarrierView carrier)
        {
            carrier.gameObject.SetActive(false);
        }

        private void OnDestroy(CarrierView carrier)
        {
            UnityEngine.Object.Destroy(carrier.gameObject);
        }
    }
}