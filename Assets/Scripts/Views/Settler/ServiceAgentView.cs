using App.Signals;
using Controllers.Ai;
using Controllers.Work;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Construction;
using System;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Views.Settler.Workers
{
    [SelectionBase]
    public class ServiceAgentView : MonoBehaviour, IWorker
    {
        public Action OnAgentReturn;

        public NpcMovementHandler MovementHandler => movementHandler;
        public float RemainingSteps => remainingSteps;
        public bool IsReturning => isReturning;

        private SignalBus signalBus;
        private NavigationGraph navigationGraph;
        private ConstructionGrid constructionGrid;

        private BuildingView assignedBuilding;
        private HashSet<Vector2Int> availableTiles;

        private float remainingSteps;
        private bool isReturning;

        private List<IService> services;

        private NpcMovementHandler movementHandler;
        private Dictionary<Node<Vector3>, int> visitCounts = new Dictionary<Node<Vector3>, int>();
        private Node<Vector3> previousNode;
        private Vector2Int? lastServicedTile;
        private float baseMovementSpeed = 1.0f;

        private static readonly Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        [Inject]
        public void Constructor(SignalBus signalBus, NavigationGraph navigationGraph, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;

            movementHandler = new NpcMovementHandler(navigationGraph, constructionGrid, baseMovementSpeed);
        }

        public void Init(ServiceAgentPayload agentPayload, List<IService> services)
        {
            assignedBuilding = agentPayload.Origin;
            availableTiles = agentPayload.AvailableTiles;
            this.services = services;

            visitCounts.Clear();
            remainingSteps = 20;
            isReturning = false;
            previousNode = null;
            lastServicedTile = null;

            if (!constructionGrid.HasRoadConnection(assignedBuilding))
            {
                signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));
                return;
            }

            var connectedRoadTiles = constructionGrid.GetAllConnectedRoadTiles(assignedBuilding);
            var randomConnectedTile = connectedRoadTiles[UnityEngine.Random.Range(0, connectedRoadTiles.Count)];
            var originPosition = navigationGraph.GetClosestNode(new Vector3(randomConnectedTile.x, 0, randomConnectedTile.y));

            transform.position = originPosition.Data;

            CalculateNextPosition();
        }

        public void Tick()
        {
            movementHandler.ModifySpeed(remainingSteps > 0 ? baseMovementSpeed / 1.25f : baseMovementSpeed);
            ProvideService();
        }

        public void ReturnToOrigin()
        {
            isReturning = true;

            var connectedRoadTiles = constructionGrid.GetAllConnectedRoadTiles(assignedBuilding);
            var randomConnectedTile = connectedRoadTiles[UnityEngine.Random.Range(0, connectedRoadTiles.Count)];
            var closestNode = navigationGraph.GetClosestNode(new Vector3(randomConnectedTile.x, 0, randomConnectedTile.y));
            var calculationResult = movementHandler.CalculateRoute(previousNode.Data, closestNode.Data);

            if (!calculationResult)
                signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));
        }

        public void FinishWork()
        {
            OnAgentReturn?.Invoke();
            OnAgentReturn = null;

            signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));
        }

        public void CalculateNextPosition()
        {
            var startNode = navigationGraph.GetClosestNode(transform.position, 1f);

            if (startNode == null)
            {
                signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));
                return;
            }

            var targetNode = GetNextNodeWeighted(startNode);

            if (targetNode == null)
            {
                signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));
                return;
            }

            if (!visitCounts.ContainsKey(targetNode))
                visitCounts[targetNode] = 0;
            visitCounts[targetNode]++;

            var calculationResult = movementHandler.CalculateRoute(startNode.Data, targetNode.Data);
            movementHandler.CurrentIndex++;

            if (!calculationResult)
            {
                signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));
                return;
            }

            previousNode = startNode;
        }

        private Node<Vector3> GetNextNodeWeighted(Node<Vector3> currentNode)
        {
            if (currentNode.Neighbors.Count == 0)
                return null;

            if (previousNode == null)
            {
                int index = UnityEngine.Random.Range(0, currentNode.Neighbors.Count);
                return currentNode.Neighbors[index];
            }

            var forward = (currentNode.Data - previousNode.Data).normalized;
            var totalWeight = 0f;
            var candidates = new List<(Node<Vector3> node, float weight)>();

            foreach (var neighbor in currentNode.Neighbors)
            {
                if (neighbor == previousNode)
                    continue;

                var dir = (neighbor.Data - currentNode.Data).normalized;
                var dot = Vector3.Dot(forward, dir);
                var directionWeight = Mathf.Clamp01((dot + 1f) * 0.5f);

                visitCounts.TryGetValue(neighbor, out int visits);
                var visitPenalty = 1f / (1f + visits);
                var weight = directionWeight * visitPenalty;

                if (weight <= 0f || !availableTiles.Contains(new Vector2Int((int)neighbor.Data.x, (int)neighbor.Data.z)))
                    continue;

                candidates.Add((neighbor, weight));
                totalWeight += weight;
            }

            if (candidates.Count == 0)
                return previousNode;

            var random = UnityEngine.Random.value * totalWeight;

            foreach (var (node, weight) in candidates)
            {
                random -= weight;
                if (random <= 0f)
                    return node;
            }

            return candidates[^1].node;
        }

        private void ProvideService()
        {
            if (remainingSteps <= 0)
                return;

            var currentPosition = movementHandler.NextPosition;
            var currentTile = new Vector2Int((int)currentPosition.x, (int)currentPosition.z);

            if (lastServicedTile == currentTile)
                return;

            lastServicedTile = currentTile;
            remainingSteps--;

            foreach (var dir in directions)
            {
                var tilePosition = currentTile + dir;
                var tile = constructionGrid.GetTileByPosition(tilePosition);
                var buildingView = tile?.BuildingView;

                if (buildingView == null)
                    continue;

                foreach (var service in services)
                {
                    if (buildingView.TryGetComponent(out IServiceReceiver serviceReceiver))
                        serviceReceiver.ReceiveService(service);
                }
            }
        }
    }

    public class ServiceAgentPayload
    {
        public BuildingView Origin { get; set; }

        public List<IService> Services { get; set; }

        public HashSet<Vector2Int> AvailableTiles { get; set; }
    }
}