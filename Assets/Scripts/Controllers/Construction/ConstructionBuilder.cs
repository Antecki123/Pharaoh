using App.Configs;
using App.Helpers;
using App.Signals;
using Cysharp.Threading.Tasks;
using Models.Ai;
using Models.Ai.Pathfinding;
using System.Linq;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionBuilder<T> : IConstruction where T : BuildingView
    {
        private T building;
        private readonly BuildingDefinition buildingDefinition;

        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;
        private readonly NavigationGraph navigationGraph;
        private readonly ConstructionConfig constructionConfig;
        private readonly ConstructionDataImporter constructionData;

        public ConstructionBuilder(SignalBus signalBus, PrefabManager prefabManager, NavigationGraph navigationGraph, ConstructionConfig constructionConfig,
            ConstructionDataImporter constructionData, BuildingDefinition buildingDefinition)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.navigationGraph = navigationGraph;
            this.constructionConfig = constructionConfig;
            this.constructionData = constructionData;
            this.buildingDefinition = buildingDefinition;
        }

        public void Initialize()
        {
            _ = LoadAssets();
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (building != null)
                {
                    CancelConstruction();
                }
            }

            if (building == null || !TryGetSnappedPosition(out Vector3 position))
                return;

            if (building != null)
            {
                building.transform.position = position;

                if (IsAvailableSpace(position))
                    building.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                else
                    building.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }

            if (Input.GetMouseButtonDown(0) && IsAvailableSpace(position))
            {
                building.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                building.PlaceBuilding();

                building = null;
                signalBus.Fire(new ConstructionSignals.ConstructionMode(buildingDefinition));
            }
        }

        private async UniTask LoadAssets()
        {
            var prefab = await AddressablesUtility.LoadAssetAsync<GameObject>(buildingDefinition.ToString());
            building = prefabManager.Instantiate<T>(prefab);
        }

        private void CancelConstruction()
        {
            if (building != null)
                Object.Destroy(building.gameObject);

            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private bool TryGetSnappedPosition(out Vector3 snappedPos)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var layerMask = 1 << 16;

            if (!Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask))
            {
                snappedPos = Vector3.zero;
                return false;
            }

            var closestDistSqr = float.MaxValue;
            var bestPos = hit.point;
            var bestForward = Vector3.forward;

            var data = constructionData.ConstructionData[buildingDefinition];

            foreach (var node in navigationGraph.Nodes)
            {
                if (node.NodeType != NodeType.Road)
                    continue;

                var roadNeighbors = node.Neighbors.Where(n => n.NodeType == NodeType.Road);
                foreach (var neighbor in roadNeighbors)
                {
                    var closestPoint = ClosestPointOnSegment(node.Data, neighbor.Data, hit.point);
                    var distSqr = (hit.point - closestPoint).sqrMagnitude;

                    if (distSqr <= data.SnapDistance * data.SnapDistance && distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;

                        var roadDir = (neighbor.Data - node.Data).normalized;
                        var forward = new Vector3(-roadDir.z, 0f, roadDir.x);
                        var toMouse = (hit.point - closestPoint).normalized;

                        if (Vector3.Dot(toMouse, forward) < 0)
                            forward = -forward;

                        var totalOffset = constructionConfig.RoadWidth / 2f + data.BuildingOffset;
                        bestPos = closestPoint + forward * totalOffset;
                        bestForward = forward;
                    }
                }
            }

            snappedPos = bestPos;

            if (closestDistSqr >= data.SnapDistance * data.SnapDistance)
                snappedPos = hit.point;

            building.transform.rotation = Quaternion.LookRotation(bestForward, Vector3.up);
            return true;
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
            var bounds = building.GetComponent<Collider>().bounds;
            Vector3 halfExtents = bounds.extents;

            Collider[] hits = Physics.OverlapBox(position, halfExtents, building.transform.rotation, layer);

            foreach (var hit in hits)
            {
                if (hit != building.GetComponent<Collider>())
                    return false;
            }

            return true;
        }
    }
}