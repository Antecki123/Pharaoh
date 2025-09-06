using App.Helpers;
using App.Signals;
using Models.Ai;
using System;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class CottageBuilder : IConstruction
    {
        public bool RoadConnectionRequired => true;

        private GameObject cottagePrafab;
        private Cottage cottage;

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private NavigationGraph navigationGraph;

        public CottageBuilder(SignalBus signalBus, PrefabManager prefabManager, NavigationGraph navigationGraph)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.navigationGraph = navigationGraph;

            cottagePrafab = Resources.Load<GameObject>("Prefabs/Cottage");
            if (cottagePrafab == null)
            {
                Debug.LogError($"Prefab 'Cottage' could not be found in 'Prefabs/Cottage'. Make sure the path and name are correct.");
                throw new NullReferenceException();
            }

            cottage = prefabManager.Instantiate<Cottage>(cottagePrafab);
            cottage.transform.position = Vector3.zero;
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (cottage != null)
                {
                    CancelConstruction();
                }
            }

            if (!TryGetSnappedPosition(out Vector3 position))
                return;

            if (cottage != null)
            {
                cottage.transform.position = position;

                if (IsAvailableSpace(position))
                    cottage.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                else
                    cottage.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }

            if (Input.GetMouseButtonDown(0) && IsAvailableSpace(position))
            {
                cottage.GetComponentInChildren<MeshRenderer>().material.color = Color.white;

                cottage = null;
                signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.Cottage));
            }
        }

        private void CancelConstruction()
        {
            UnityEngine.Object.Destroy(cottage.gameObject);
            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private bool TryGetSnappedPosition(out Vector3 snappedPos)
        {
            var roadWidth = .5f;
            var buildingOffset = 0.5f;
            var snapDistance = 1f;
            var layerMask = 1 << 16;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
            {
                var closestDistSqr = float.MaxValue;
                var bestPos = hit.point;
                var bestForward = Vector3.forward;

                foreach (var node in navigationGraph.Nodes)
                {
                    foreach (var neighbor in node.Neighbors)
                    {
                        Vector3 closest = ClosestPointOnSegment(node.Position, neighbor.Position, hit.point);
                        var distSqr = (hit.point - closest).sqrMagnitude;

                        if (distSqr < closestDistSqr && distSqr <= snapDistance * snapDistance)
                        {
                            closestDistSqr = distSqr;

                            Vector3 roadDir = (neighbor.Position - node.Position).normalized;
                            Vector3 forward = new Vector3(roadDir.z, 0, -roadDir.x);
                            Vector3 toMouse = (hit.point - closest).normalized;
                            if (Vector3.Dot(toMouse, forward) < 0)
                                forward = -forward;

                            var totalOffset = roadWidth / 2f + buildingOffset;

                            bestPos = closest + forward * totalOffset;
                            bestForward = forward;
                        }
                    }
                }

                snappedPos = bestPos;

                if (closestDistSqr < float.MaxValue)
                {
                    cottage.transform.rotation = Quaternion.LookRotation(bestForward, Vector3.up);
                }

                return true;
            }

            snappedPos = Vector3.zero;
            return false;
        }

        private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }

        private bool IsAvailableSpace(Vector3 position)
        {
            var layer = 1 << 17;
            var bounds = cottage.GetComponent<Collider>().bounds;
            Vector3 halfExtents = bounds.extents;

            Collider[] hits = Physics.OverlapBox(position, halfExtents, cottage.transform.rotation, layer);

            foreach (var hit in hits)
            {
                if (hit != cottage.GetComponent<Collider>())
                    return false;
            }

            return true;
        }
    }
}