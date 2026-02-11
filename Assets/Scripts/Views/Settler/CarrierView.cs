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
    public class CarrierView : MonoBehaviour
    {
        public event Action OnTasksFinished;

        private Animator animator;
        private NavigationGraph navigationGraph;
        private ConstructionGrid constructionGrid;

        private Queue<CarrierTask> carrierTasks = new Queue<CarrierTask>();
        private CarrierTask currentTask;
        private CommodityModel carriedCommodity;

        private NpcMovementHandler movementHandler;
        private float movementSpeed = 5f;

        [Inject]
        public void Constructor(NavigationGraph navigationGraph, ConstructionGrid constructionGrid)
        {
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;

            animator = GetComponentInChildren<Animator>();
        }

        public void Init(Queue<CarrierTask> carrierTasks)
        {
            this.carrierTasks = carrierTasks;

            movementHandler = new NpcMovementHandler(navigationGraph, constructionGrid);

            StartNextTask();
        }

        private void OnDestroy()
        {
            OnTasksFinished?.Invoke();
        }

        private void Update()
        {
            animator.SetBool("CarryingDelivery", carriedCommodity != null);
            var currentSpeed = carriedCommodity != null ? movementSpeed / 2 : movementSpeed;

            if (movementHandler.Waypoints.Count == 0 || currentTask == null)
                return;

            var nextPos = movementHandler.NextPosition;
            transform.position = Vector3.MoveTowards(transform.position, nextPos, currentSpeed * Time.deltaTime);

            var direction = (nextPos - transform.position).normalized;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 15f);
            }

            if (Vector3.Distance(transform.position, nextPos) <= 0.1f)
            {
                movementHandler.CurrentIndex++;

                if (movementHandler.CurrentIndex >= movementHandler.Waypoints.Count)
                {
                    if (carriedCommodity != null && currentTask.Target != null)
                    {
                        currentTask.Target.DeliverCommodity(carriedCommodity);
                        carriedCommodity = null;
                    }

                    StartNextTask();
                }
            }
        }

        private void StartNextTask()
        {
            if (carrierTasks.Count == 0)
            {
                Destroy(gameObject);
                return;
            }

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