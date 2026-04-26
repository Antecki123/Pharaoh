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

        private Queue<CarrierTask> carrierTasks = new Queue<CarrierTask>();
        private CarrierTask currentTask;
        private CommodityModel carriedCommodity;

        private NpcMovementHandler movementHandler;
        private readonly float baseMovementSpeed = 1.0f;

        [Inject]
        public void Constructor(SignalBus signalBus, NavigationGraph navigationGraph, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;

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
                currentTask.Target.DeliverCommodity(carriedCommodity);
                carriedCommodity = null;
            }

            if (carrierTasks.Count == 0)
            {
                OnTasksFinished?.Invoke();
                OnTasksFinished = null;

                signalBus.Fire(new WorkplaceSignals.ReturnCarrier(this));
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
                        currentTask.Origin.GetReservationable().RemoveReservation(currentTask.ReservationId.Value);

                    carriedCommodity = commodity;
                }
            }

            var calculationResult = movementHandler.CalculateRoute(currentTask.Origin.GetBuildingView(), currentTask.Target.GetBuildingView());
            if (calculationResult)
                transform.position = movementHandler.Waypoints[0];
        }
    }

    public class CarrierTask
    {
        public ISupplyTarget Origin { get; }

        public ISupplyTarget Target { get; }

        public CommodityModel Commodity { get; }

        public Guid? ReservationId { get; }

        public CarrierTask(ISupplyTarget origin, ISupplyTarget target, CommodityModel commodity, Guid? reservationId = null)
        {
            Origin = origin;
            Target = target;
            Commodity = commodity;
            ReservationId = reservationId;
        }
    }
}