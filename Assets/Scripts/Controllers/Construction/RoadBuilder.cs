using App.Configs;
using App.Helpers;
using App.Signals;
using Cysharp.Threading.Tasks;
using Models.Ai;
using Models.Ai.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Road;
using Zenject;

namespace Controllers.Construction
{
    public class RoadBuilder : IConstruction
    {
        private List<Vector3> segmentsPositions = new List<Vector3>();
        private Vector3? startPosition;
        private Vector3? endPosition;

        private GameObject roadPreview;
        private GameObject pointer;
        private Material previewRoadMaterial;

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private NavigationGraph navigationGraph;
        private ConstructionConfig constructionConfig;

        public RoadBuilder(SignalBus signalBus, PrefabManager prefabManager, NavigationGraph navigationGraph, ConstructionConfig constructionConfig)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.navigationGraph = navigationGraph;
            this.constructionConfig = constructionConfig;
        }

        public void Initialize()
        {
            var loadedAsset = Resources.Load<GameObject>("Prefabs/RoadPreview");
            if (loadedAsset == null)
            {
                Debug.LogError($"Prefab 'roadPreview' could not be found in 'Prefabs/RoadPreview'. Make sure the path and name are correct.");
                throw new NullReferenceException();
            }

            previewRoadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadPreview = UnityEngine.Object.Instantiate(loadedAsset);

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pointer.transform.localScale = new Vector3(1f, .001f, 1f);
            pointer.name = "Pointer";
            pointer.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.cyan };
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (startPosition != null)
                {
                    startPosition = null;
                    endPosition = null;
                    segmentsPositions.Clear();

                    var lineRenderer = roadPreview.GetComponent<LineRenderer>();
                    lineRenderer.positionCount = 0;
                }
                else
                {
                    CancelConstruction();
                }
            }

            if (!TryGetSnappedPosition(out Vector3 position))
                return;

            pointer.transform.position = new Vector3(position.x, .1f, position.z);

            if (Input.GetMouseButtonDown(0))
            {
                if (startPosition == null)
                {
                    SelectFirstPoint(position);
                }
                else if (endPosition == null && !IntersectsExistingRoad(startPosition.Value, position) &&
                    IsValidRoadAngle(startPosition.Value, position, navigationGraph.GetNeighborsPosition(startPosition.Value), constructionConfig.MinimumRoadAngle))
                {
                    CreateRoad(position);
                }
            }

            if (startPosition != null && endPosition == null)
            {
                if (roadPreview != null)
                {
                    var lineRenderer = roadPreview.GetComponent<LineRenderer>();
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, startPosition.Value);
                    lineRenderer.SetPosition(1, position);

                    if (navigationGraph.Contains(startPosition.Value))
                    {
                        if (IsValidRoadAngle(startPosition.Value, position, navigationGraph.GetNeighborsPosition(startPosition.Value),
                            constructionConfig.MinimumRoadAngle) && !IntersectsExistingRoad(startPosition.Value, position))
                            previewRoadMaterial.color = Color.green;
                        else
                            previewRoadMaterial.color = Color.red;
                    }
                    else
                        previewRoadMaterial.color = Color.green;

                    lineRenderer.material = previewRoadMaterial;

                    segmentsPositions = GenerateSegments(startPosition.Value, position);
                }
            }
        }

        private void CancelConstruction()
        {
            startPosition = null;
            endPosition = null;
            segmentsPositions.Clear();

            if (roadPreview != null)
            {
                UnityEngine.Object.Destroy(roadPreview);
                roadPreview = null;
            }

            if (pointer != null)
                UnityEngine.Object.Destroy(pointer);

            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private void SelectFirstPoint(Vector3 worldPos)
        {
            startPosition = new Vector3(worldPos.x, .01f, worldPos.z);

            var lineRenderer = roadPreview.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPosition.Value);
            lineRenderer.SetPosition(1, startPosition.Value);
        }

        private void CreateRoad(Vector3 position)
        {
            endPosition = new Vector3(position.x, .01f, position.z);

            var routePrefab = Resources.Load<RoadView>("Prefabs/RoadView");
            if (routePrefab == null)
            {
                Debug.LogError($"Prefab 'roadPrefab' could not be found in 'Prefabs/RoadView'. Make sure the path and name are correct.");
                throw new NullReferenceException();
            }

            var segmentNodes = new List<Node<Vector3>>();

            for (int i = 0; i < segmentsPositions.Count; i++)
            {
                var nodePosition = segmentsPositions[i];
                Node<Vector3> node;

                node = navigationGraph.GetNode(nodePosition);
                if (node == null)
                {
                    var nodeType = NodeType.Road;
                    node = new Node<Vector3>(
                        nodePosition,
                        nodeType,
                        (a, b) =>
                        {
                            float dist = Vector3.Distance(a.Data, b.Data);
                            float multiplier = navigationGraph.MovementCost[nodeType];
                            return dist * multiplier;
                        },
                        (a, goal) => Vector3.Distance(a.Data, goal.Data)
                    );

                    navigationGraph.Nodes.Add(node);
                }

                segmentNodes.Add(node);
            }

            for (int i = 0; i < segmentNodes.Count - 1; i++)
            {
                var current = segmentNodes[i];
                var next = segmentNodes[i + 1];

                if (!current.Neighbors.Contains(next))
                    current.Neighbors.Add(next);

                if (!next.Neighbors.Contains(current))
                    next.Neighbors.Add(current);

                var routeView = prefabManager.Instantiate<RoadView>(routePrefab.gameObject);
                routeView.Init(current.Data, next.Data);
            }

            float connectionRange = 3f;

            foreach (var roadNode in segmentNodes)
            {
                var nearbyTerrainNodes = navigationGraph.Nodes
                    .Where(n => n.NodeType == NodeType.Terrain)
                    .Where(n => Vector3.Distance(n.Data, roadNode.Data) <= connectionRange);

                foreach (var terrainNode in nearbyTerrainNodes)
                {
                    if (!roadNode.Neighbors.Contains(terrainNode))
                        roadNode.Neighbors.Add(terrainNode);

                    if (!terrainNode.Neighbors.Contains(roadNode))
                        terrainNode.Neighbors.Add(roadNode);
                }
            }

            startPosition = endPosition;
            endPosition = null;
        }

        private bool TryGetSnappedPosition(out Vector3 snappedPos)
        {
            var snapDistance = 1f;
            var closestDistSqr = float.MaxValue;
            Node<Vector3> closestNode = null;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = 1 << 16;

            if (!Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask))
            {
                snappedPos = Vector3.zero;
                return false;
            }

            foreach (var node in navigationGraph.Nodes)
            {
                if (node.NodeType != NodeType.Road)
                    continue;

                var distSqr = (hit.point - node.Data).sqrMagnitude;
                if (distSqr < closestDistSqr && distSqr <= snapDistance * snapDistance)
                {
                    closestDistSqr = distSqr;
                    closestNode = node;
                }
            }

            if (closestNode != null)
                snappedPos = closestNode.Data;
            else
                snappedPos = hit.point;

            return true;
        }

        private bool IsValidRoadAngle(Vector3 nodePos, Vector3 newPos, IEnumerable<Vector3> neighbors, float minAngle)
        {
            var dirNew = (newPos - nodePos).normalized;

            foreach (var neighbor in neighbors)
            {
                var dirExisting = (neighbor - nodePos).normalized;
                float angle = Vector3.Angle(dirNew, dirExisting);

                if (angle < minAngle)
                    return false;
            }

            return true;
        }

        private bool IntersectsExistingRoad(Vector3 startPos, Vector3 endPos)
        {
            var newA = new Vector2(startPos.x, startPos.z);
            var newB = new Vector2(endPos.x, endPos.z);

            foreach (var node in navigationGraph.Nodes)
            {
                if (node.NodeType != NodeType.Road)
                    continue;

                var roadNeighbors = node.Neighbors.Where(n => n.NodeType == NodeType.Road);
                foreach (var neighbor in roadNeighbors)
                {
                    if (node.Id.CompareTo(neighbor.Id) > 0)
                        continue;

                    if (ApproximatelySame(node.Data, startPos) || ApproximatelySame(node.Data, endPos) ||
                        ApproximatelySame(neighbor.Data, startPos) || ApproximatelySame(neighbor.Data, endPos))
                        continue;

                    var existingA = new Vector2(node.Data.x, node.Data.z);
                    var existingB = new Vector2(neighbor.Data.x, neighbor.Data.z);

                    if (DoSegmentsIntersect(newA, newB, existingA, existingB))
                        return true;
                }
            }

            return false;
        }

        private bool DoSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            var d1 = Direction(p1, p2, q1);
            var d2 = Direction(p1, p2, q2);
            var d3 = Direction(q1, q2, p1);
            var d4 = Direction(q1, q2, p2);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            return false;
        }

        private float Direction(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
        }

        private bool ApproximatelySame(Vector3 a, Vector3 b, float tolerance = 0.01f)
        {
            return Vector3.Distance(a, b) <= tolerance;
        }

        private List<Vector3> GenerateSegments(Vector3 startPos, Vector3 endPos)
        {
            var points = new List<Vector3>();
            var direction = (endPos - startPos).normalized;
            var distance = Vector3.Distance(startPos, endPos);

            int steps = Mathf.FloorToInt(distance / constructionConfig.SegmentSpacing);

            for (int i = 0; i <= steps; i++)
            {
                var point = startPos + direction * (i * constructionConfig.SegmentSpacing);
                points.Add(point);
            }

            float lastDist = Vector3.Distance(points[^1], endPos);

            if (lastDist < constructionConfig.MinimumSpacing)
            {
                points[^1] = endPos;
            }
            else
            {
                points.Add(endPos);
            }

            return points;
        }


        private async UniTask LoadAssets()
        {
            roadPreview = await AddressablesUtility.LoadAssetAsync<GameObject>("RoadPreview");
        }
    }
}