using Controllers.Ai.Strategy;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Settler;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Views.Settler
{
    [SelectionBase]
    public class SettlerView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private LineRenderer lineRenderer;

        private SettlerModel settlerModel;
        private Strategy strategy;

        private IPathfindingBrain<Vector3> pathfinding;
        [Inject] private NavigationGraph navigationGraph;


        private List<Vector3> waypoints = new List<Vector3>();
        private int currentIndex = 0;

        public void Init(SettlerModel settlerModel)
        {
            this.settlerModel = settlerModel;

            //strategy = new StrategyFactory(this, pathfinding, null).GetStrategy(StrategyDefinition.None);

            Node<Vector3> startNode;
            Node<Vector3> targetNode;
            var nodesList = navigationGraph.Nodes.ToList();

            do
            {
                startNode = nodesList[Random.Range(0, nodesList.Count)];
                targetNode = nodesList[Random.Range(0, nodesList.Count)];
            }
            while (startNode == targetNode);

            transform.position = startNode.Data;

            pathfinding = new DStarLite<Vector3>();
            pathfinding.Initialize(navigationGraph.Nodes, startNode, targetNode);
            waypoints = pathfinding
                .GetPath()
                .Select(x => x.Data)
                .ToList();

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = waypoints.Count;
            lineRenderer.SetPositions(waypoints
                    .ConvertAll(p => new Vector3(p.x, 0.1f, p.z))
                    .ToArray());
        }

        public void Tick()
        {
            if (waypoints.Count == 0) return;

            var targetPos = waypoints[currentIndex];

            transform.position = Vector3.MoveTowards(transform.position, targetPos, 10f * Time.deltaTime);

            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f);
            }

            if (Vector3.Distance(transform.position, targetPos) <= .1f)
            {
                currentIndex = (currentIndex + 1) % waypoints.Count;
            }

            if (currentIndex == waypoints.Count - 1)
            {
                Node<Vector3> startNode = navigationGraph.GetNode(targetPos);
                Node<Vector3> targetNode;

                currentIndex = 0;
                var nodesList = navigationGraph.Nodes.ToList();
                do
                {
                    targetNode = nodesList[Random.Range(0, nodesList.Count)];
                }
                while (startNode == targetNode);

                pathfinding.Initialize(navigationGraph.Nodes, startNode, targetNode);
                waypoints = pathfinding
                    .GetPath()
                    .Select(x => x.Data)
                    .ToList();

                lineRenderer.positionCount = waypoints.Count;
                lineRenderer.SetPositions(waypoints
                        .ConvertAll(p => new Vector3(p.x, 0.1f, p.z))
                        .ToArray());
            }

            //strategy?.Tick();
        }
    }
}