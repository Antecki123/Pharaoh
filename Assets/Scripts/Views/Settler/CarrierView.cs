using Controllers.Work;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Economy;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private Queue<CarrierTask> carrierTasks = new Queue<CarrierTask>();
        private CarrierTask currentTask;
        private CommodityModel carriedCommodity;

        private List<Vector3> waypoints = new List<Vector3>();
        private int currentIndex = 0;
        private float movementSpeed = 13f;

        [Inject]
        public void Constructor(NavigationGraph navigationGraph)
        {
            this.navigationGraph = navigationGraph;

            animator = GetComponentInChildren<Animator>();
        }

        public void Init(Queue<CarrierTask> carrierTasks)
        {
            this.carrierTasks = carrierTasks;

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

            if (waypoints.Count == 0 || currentTask == null)
                return;

            var targetPos = waypoints[currentIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            var direction = (targetPos - transform.position).normalized;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 15f);
            }

            if (Vector3.Distance(transform.position, targetPos) <= 0.1f)
            {
                currentIndex++;

                if (currentIndex >= waypoints.Count)
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

            CalculateWaypoints(currentTask.Origin, currentTask.Target);
        }

        private void CalculateWaypoints(ISupplyTarget origin, ISupplyTarget target)
        {
            var nodesList = navigationGraph.Nodes.ToList();
            var startNode = navigationGraph.GetNode(origin.GetEntrancePosition());
            var endNode = navigationGraph.GetNode(target.GetEntrancePosition());

            transform.position = startNode.Data;

            var dStar = new DStarLite<Vector3>();
            dStar.Initialize(nodesList, startNode, endNode);
            var path = dStar.GetPath();

            foreach (var position in path)
                waypoints.Add(position.Data);
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