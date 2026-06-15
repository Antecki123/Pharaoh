using App.Signals;
using Controllers.Ai;
using Controllers.Work;
using Models.Ai;
using Models.Construction;
using Models.Economy;
using Models.Work;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Views.Settler.Workers
{
    [SelectionBase]
    public class CarrierView : MonoBehaviour, IWorker
    {
        public event Action OnTasksFinished;

        public NpcMovementHandler MovementHandler => movementHandler;

        private Animator animator;
        private SignalBus signalBus;
        private SupplyModel supplyModel;

        private Queue<CarrierTask> carrierTasks = new();
        private CarrierTask currentTask;
        private CommodityModel carriedCommodity;

        private NpcMovementHandler movementHandler;
        private readonly float baseMovementSpeed = 1.0f;

        [Inject]
        public void Constructor(SignalBus signalBus, SupplyModel supplyModel, NavigationGraph navigationGraph,
            ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;

            movementHandler = new NpcMovementHandler(navigationGraph, constructionGrid, baseMovementSpeed);
            animator = GetComponentInChildren<Animator>();
        }

        public void Init(Queue<CarrierTask> carrierTasks)
        {
            this.carrierTasks = carrierTasks;
            StartNextTask();
        }

        public void Tick()
        {
            animator.SetBool("CarryingDelivery", carriedCommodity != null);
            movementHandler.ModifySpeed(carriedCommodity != null ? baseMovementSpeed / 2 : baseMovementSpeed);
        }

        public void FinishTask()
        {
            if (carriedCommodity != null && currentTask.Target != null)
            {
                currentTask.Target.AddCommodity(carriedCommodity);
                carriedCommodity = null;
            }

            if (carrierTasks.Count == 0)
            {
                FinishWork();
                return;
            }
            else
            {
                StartNextTask();
            }
        }

        private void StartNextTask()
        {
            currentTask = carrierTasks.Dequeue();
            carriedCommodity = null;

            if (currentTask.Origin != null && currentTask.Commodity != null)
            {
                var commodity = currentTask.Commodity;
                var result = currentTask.Origin.TryPickCommodity(ref commodity);
                if (result)
                {
                    if (currentTask.ReservationId.HasValue)
                        currentTask.Origin.RemoveReservation(currentTask.ReservationId.Value);

                    carriedCommodity = commodity;
                }
            }

            var originView = supplyModel.GetBuildingView(currentTask.Origin);
            var targetView = supplyModel.GetBuildingView(currentTask.Target);

            if (originView == null || targetView == null)
            {
                FinishWork();
                return;
            }

            var calculationResult = movementHandler.CalculateRoute(originView, targetView);
            if (calculationResult)
                transform.position = movementHandler.Waypoints[0];
        }

        private void FinishWork()
        {
            OnTasksFinished?.Invoke();
            OnTasksFinished = null;

            signalBus.Fire(new WorkplaceSignals.ReturnCarrier(this));
        }
    }
}