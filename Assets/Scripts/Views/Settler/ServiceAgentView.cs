using App.Signals;
using Controllers.Ai;
using Controllers.Work;
using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Construction;
using Models.Habitation;
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
        public float RemainingCapacity => remainingCapacity;
        public bool IsReturning => isReturning;

        private SignalBus signalBus;
        private NavigationGraph navigationGraph;
        private ConstructionGrid constructionGrid;

        private BuildingView assignedBuilding;
        private HabitationRequirementDefinition habitationRequirement;
        private HashSet<Vector2Int> availableTiles;

        private float remainingCapacity = 100f;
        private bool isReturning;

        private NpcMovementHandler movementHandler;
        private Node<Vector3> previousNode;
        private float baseMovementSpeed = 1.0f;

        private Dictionary<Node<Vector3>, int> visitCounts = new Dictionary<Node<Vector3>, int>();
        private Vector3? lastVisitedTile;

        [Inject]
        public void Constructor(SignalBus signalBus, NavigationGraph navigationGraph, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;

            movementHandler = new NpcMovementHandler(navigationGraph, constructionGrid, baseMovementSpeed);
        }

        public void Init(ServiceAgentPayload agentPayload)
        {
            assignedBuilding = agentPayload.Origin;
            habitationRequirement = agentPayload.HabitationRequirementDefinition;
            availableTiles = agentPayload.AvailableTiles;

            visitCounts.Clear();
            remainingCapacity = 100f;
            isReturning = false;

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
            movementHandler.ModifySpeed(remainingCapacity > 0 ? baseMovementSpeed / 1.25f : baseMovementSpeed);
            SatisfyResident();
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
            var targetNode = GetNextNodeWeighted(startNode);

            if (!visitCounts.ContainsKey(targetNode))
                visitCounts[targetNode] = 0;

            visitCounts[targetNode]++;

            if (startNode == null || targetNode == null)
                signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));

            var calculationResult = movementHandler.CalculateRoute(startNode.Data, targetNode.Data);

            movementHandler.CurrentIndex++;

            if (!calculationResult)
                signalBus.Fire(new WorkplaceSignals.ReturnServiceAgent(this));
        }

        private Node<Vector3> GetNextNodeWeighted(Node<Vector3> currentNode)
        {
            if (currentNode.Neighbors.Count == 0)
                return null;

            if (movementHandler.Waypoints.Count > 0)
                previousNode = navigationGraph.GetNode(movementHandler.Waypoints[0]);

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

        private void SatisfyResident()
        {
            var currentTile = movementHandler.NextPosition;
            if (currentTile != lastVisitedTile)
            {
                lastVisitedTile = currentTile;

                var baseServiceValue = 10f;
                var directions = new[]
                {
                    Vector2Int.left,
                    Vector2Int.right,
                    Vector2Int.up,
                    Vector2Int.down,
                };

                var current = new Vector2Int((int)currentTile.x, (int)currentTile.z);

                foreach (var dir in directions)
                {
                    if (remainingCapacity <= 0)
                        break;

                    var tilePosition = current + dir;
                    var tile = constructionGrid.GetTileByPosition(tilePosition);

                    var buildingView = tile?.BuildingView;
                    if (buildingView == null)
                        continue;

                    if (buildingView.TryGetComponent(out IServiceReceiver serviceReceiver))
                    {
                        var residual = serviceReceiver.SatisfyResidentNeeds(habitationRequirement, baseServiceValue);
                        remainingCapacity -= baseServiceValue - residual;
                    }
                }
            }
        }
    }

    public class ServiceAgentPayload
    {
        public BuildingView Origin { get; set; }

        public HabitationRequirementDefinition HabitationRequirementDefinition { get; set; }

        public HashSet<Vector2Int> AvailableTiles { get; set; }
    }
}