using Controllers.Work;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Economy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Views.Settler.Workers
{
    [SelectionBase]
    public class CarrierView : MonoBehaviour
    {
        private Animator animator;
        private NavigationGraph navigationGraph;

        private CommodityModel carriedCommodity;
        private ISupplyTarget origin;
        private ISupplyTarget target;

        private List<Vector3> waypoints = new List<Vector3>();
        private int currentIndex = 0;
        private float movementSpeed = 3f;

        [Inject]
        public void Constructor(NavigationGraph navigationGraph)
        {
            this.navigationGraph = navigationGraph;
            animator = GetComponentInChildren<Animator>();
        }

        public void Init(CommodityModel carriedCommodity, ISupplyTarget origin, ISupplyTarget target)
        {
            this.carriedCommodity = carriedCommodity;
            this.origin = origin;
            this.target = target;

            var result = origin.TryPickCommodity(carriedCommodity);
            if (result)
            {
                CalculateWaypoints(origin, target);
            }
        }

        private void Update()
        {
            animator.SetBool("CarryingDelivery", carriedCommodity != null);
            var currentSpeed = carriedCommodity != null ? movementSpeed / 2 : movementSpeed;

            if (waypoints.Count == 0)
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
                currentIndex = (currentIndex + 1) % waypoints.Count;
            }

            if (currentIndex == waypoints.Count - 1)
            {
                if (carriedCommodity != null)
                {
                    target.DeliverCommodity(carriedCommodity);
                    carriedCommodity = null;
                    CalculateWaypoints(target, origin);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
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
}