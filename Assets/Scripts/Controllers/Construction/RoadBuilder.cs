using App.Configs;
using App.Helpers;
using App.Signals;
using Models.Ai;
using System;
using System.Collections.Generic;
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

            var loadedAsset = Resources.Load<GameObject>("Prefabs/RoadPreview");
            if (loadedAsset == null)
            {
                Debug.LogError($"Prefab 'roadPreview' could not be found in 'Prefabs/RoadPreview'. Make sure the path and name are correct.");
                throw new NullReferenceException();
            }

            roadPreview = UnityEngine.Object.Instantiate(loadedAsset);

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pointer.transform.localScale = new Vector3(.5f, .001f, .5f);
            pointer.name = "Pointer";

            previewRoadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        public void Initialize()
        {

        }

        public void Tick()
        {
            if (TryGetSnappedPosition(out Vector3 snappedPos) && endPosition == null)
                pointer.transform.position = new Vector3(snappedPos.x, .01f, snappedPos.z);

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

            if (Input.GetMouseButtonDown(0))
            {
                if (TryGetSnappedPosition(out Vector3 worldPos))
                {
                    if (startPosition == null)
                    {
                        SelectFirstPoint(worldPos);
                    }
                    else if (endPosition == null && !IntersectsExistingRoad(startPosition.Value, worldPos) &&
                        IsValidRoadAngle(startPosition.Value, worldPos, navigationGraph.GetNeighborsPosition(startPosition.Value), constructionConfig.MinimumRoadAngle))
                    {
                        SelectEndPoint(worldPos);
                    }
                }
            }

            if (startPosition != null && endPosition == null)
            {
                if (TryGetSnappedPosition(out Vector3 previewPos) && roadPreview != null)
                {
                    var lineRenderer = roadPreview.GetComponent<LineRenderer>();
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, startPosition.Value);
                    lineRenderer.SetPosition(1, previewPos);

                    if (navigationGraph.Contains(startPosition.Value))
                    {
                        if (IsValidRoadAngle(startPosition.Value, previewPos, navigationGraph.GetNeighborsPosition(startPosition.Value),
                            constructionConfig.MinimumRoadAngle) && !IntersectsExistingRoad(startPosition.Value, previewPos))
                            previewRoadMaterial.color = Color.green;
                        else
                            previewRoadMaterial.color = Color.red;
                    }
                    else
                        previewRoadMaterial.color = Color.green;

                    lineRenderer.material = previewRoadMaterial;

                    segmentsPositions = GenerateSegments(startPosition.Value, previewPos);
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

        private void SelectEndPoint(Vector3 worldPos)
        {
            endPosition = new Vector3(worldPos.x, .01f, worldPos.z);
            UnityEngine.Object.Destroy(pointer);

            var routePrefab = Resources.Load<RoadView>("Prefabs/RoadView");
            if (routePrefab == null)
            {
                Debug.LogError($"Prefab 'roadPrefab' could not be found in 'Prefabs/RoadView'. Make sure the path and name are correct.");
                throw new NullReferenceException();
            }

            for (int i = 0; i < segmentsPositions.Count; i++)
            {
                var position = segmentsPositions[i];
                var currentNode = navigationGraph.GetNode(position);

                if (currentNode == null)
                {
                    currentNode = new RoadNode(position);
                    navigationGraph.Nodes.Add(currentNode);
                }

                if (i < segmentsPositions.Count - 1)
                {
                    var nextPos = segmentsPositions[i + 1];
                    var nextNode = navigationGraph.GetNode(nextPos);

                    if (nextNode == null)
                    {
                        nextNode = new RoadNode(nextPos);
                        navigationGraph.Nodes.Add(nextNode);
                    }

                    if (!currentNode.Neighbors.Contains(nextNode))
                        currentNode.Neighbors.Add(nextNode);

                    if (!nextNode.Neighbors.Contains(currentNode))
                        nextNode.Neighbors.Add(currentNode);

                    var routeView = prefabManager.Instantiate<RoadView>(routePrefab.gameObject);
                    routeView.Init(currentNode.Position, nextNode.Position);
                }
            }

            startPosition = endPosition;
            endPosition = null;
            pointer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pointer.transform.localScale = new Vector3(1f, .01f, 1f);
            pointer.name = "Pointer";
        }

        private bool TryGetSnappedPosition(out Vector3 snappedPos)
        {
            var snapDistance = 0.5f;
            var closestDistSqr = float.MaxValue;
            RoadNode closestNode = null;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = 1 << 16;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
            {
                foreach (var node in navigationGraph.Nodes)
                {
                    float distSqr = (hit.point - node.Position).sqrMagnitude;
                    if (distSqr < closestDistSqr && distSqr <= snapDistance * snapDistance)
                    {
                        closestDistSqr = distSqr;
                        closestNode = node;
                    }
                }

                if (closestNode != null)
                    snappedPos = closestNode.Position;
                else
                    snappedPos = hit.point;

                return true;
            }

            snappedPos = Vector3.zero;
            return false;
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
                foreach (var neighbor in node.Neighbors)
                {
                    if (node.NodeId.CompareTo(neighbor.NodeId) > 0)
                        continue;

                    if (ApproximatelySame(node.Position, startPos) || ApproximatelySame(node.Position, endPos) ||
                        ApproximatelySame(neighbor.Position, startPos) || ApproximatelySame(neighbor.Position, endPos))
                        continue;

                    var existingA = new Vector2(node.Position.x, node.Position.z);
                    var existingB = new Vector2(neighbor.Position.x, neighbor.Position.z);

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

            Vector3? lastAdded = null;

            for (int i = 0; i <= steps; i++)
            {
                var point = startPos + direction * (i * constructionConfig.SegmentSpacing);

                if (lastAdded == null || Vector3.Distance(lastAdded.Value, point) >= constructionConfig.MinimumSpacing)
                {
                    points.Add(point);
                    lastAdded = point;
                }
            }

            if (Vector3.Distance(points[^1], endPos) >= constructionConfig.MinimumSpacing)
                points.Add(endPos);
            else
                points[^1] = endPos;

            return points;
        }
    }
}