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

        private SignalBus signalBus;
        private NavigationGraph navigationGraph;

        public bool RoadConnectionRequired => false;

        public RoadBuilder(SignalBus signalBus, NavigationGraph navigationGraph)
        {
            this.signalBus = signalBus;
            this.navigationGraph = navigationGraph;

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
        }

        public void Tick()
        {
            if (TryGetSnappedPosition(out Vector3 snappedPos) && endPosition == null)
                pointer.transform.position = new Vector3(snappedPos.x, .01f, snappedPos.z);

            if (Input.GetMouseButtonDown(1))
            {
                startPosition = null;
                endPosition = null;
                segmentsPositions.Clear();

                if (roadPreview != null)
                {
                    UnityEngine.Object.Destroy(roadPreview);
                    UnityEngine.Object.Destroy(pointer);
                }

                signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (TryGetSnappedPosition(out Vector3 worldPos))
                {
                    // SELECT FIRST NODE
                    if (startPosition == null)
                    {
                        startPosition = new Vector3(worldPos.x, .01f, worldPos.z);

                        var lineRenderer = roadPreview.GetComponent<LineRenderer>();
                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, startPosition.Value);
                        lineRenderer.SetPosition(1, startPosition.Value);
                    }
                    // CREATE ROAD
                    else if (endPosition == null)
                    {
                        endPosition = new Vector3(worldPos.x, .01f, worldPos.z);
                        UnityEngine.Object.Destroy(roadPreview);
                        UnityEngine.Object.Destroy(pointer);
                        roadPreview = null;

                        var routePrefab = Resources.Load<RoadView>("Prefabs/RoadView");
                        if (routePrefab == null)
                        {
                            Debug.LogError($"Prefab 'routePrefab' could not be found in 'Prefabs/RoadView'. Make sure the path and name are correct.");
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

                                var routeView = UnityEngine.Object.Instantiate(routePrefab);
                                routeView.navigationGraph = navigationGraph;
                                routeView.Init(currentNode.Position, nextNode.Position);
                            }
                        }
                    }
                    else
                    {
                        startPosition = new Vector3(worldPos.x, .01f, worldPos.z);
                        endPosition = null;
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

                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = Color.green;

                    lineRenderer.material = mat;

                    segmentsPositions = GenerateSegments(startPosition.Value, previewPos);
                }
            }
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

        private List<Vector3> GenerateSegments(Vector3 startPos, Vector3 endPos)
        {
            var points = new List<Vector3>();

            var direction = (endPos - startPos).normalized;
            var distance = Vector3.Distance(startPos, endPos);
            int steps = Mathf.FloorToInt(distance / navigationGraph.SegmentSpacing);

            Vector3? lastAdded = null;

            for (int i = 0; i <= steps; i++)
            {
                var point = startPos + direction * (i * navigationGraph.SegmentSpacing);

                if (lastAdded == null || Vector3.Distance(lastAdded.Value, point) >= navigationGraph.MinimumSpacing)
                {
                    points.Add(point);
                    lastAdded = point;
                }
            }

            if (Vector3.Distance(points[^1], endPos) >= navigationGraph.MinimumSpacing)
                points.Add(endPos);
            else
                points[^1] = endPos;

            return points;
        }
    }
}