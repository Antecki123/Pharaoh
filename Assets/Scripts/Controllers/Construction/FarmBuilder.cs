using App.Helpers;
using App.Signals;
using Models.Ai;
using Models.Ai.Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class FarmBuilder : IConstruction
    {
        public class Factory : PlaceholderFactory<FarmBuilder> { }

        private readonly SignalBus signalBus;

        private Transform constructionsContainer;
        private PrefabManager prefabManager;
        private NavigationGraph navigationGraph;
        private BuildingDefinition buildingDefinition;

        private List<Vector3> farmVertices = new List<Vector3>();
        private FarmPreviewView farmPreview;

        private GameObject pointer;
        private Camera mainCamera;

        public FarmBuilder(SignalBus signalBus, PrefabManager prefabManager, NavigationGraph navigationGraph)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.navigationGraph = navigationGraph;
        }

        public void Setup(BuildingDefinition buildingDefinition, Transform constructionsContainer)
        {
            this.buildingDefinition = buildingDefinition;
            this.constructionsContainer = constructionsContainer;
            mainCamera = Camera.main;
        }

        public void Initialize()
        {
            CreatePointer();
            CreateFarmPreview();
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (farmVertices.Count > 0)
                    RestartConstruction();
                else
                    CancelConstruction();
            }

            if (!TryGetSnappedPosition(out Vector3 position))
                return;

            if (pointer != null)
                pointer.transform.position = new Vector3(position.x, .1f, position.z);

            if (Input.GetMouseButtonDown(0))
            {
                if (farmVertices.Count == 0 || position != farmVertices[0])
                {
                    farmVertices.Add(position);
                    farmPreview.AddVerticleView(position);
                    return;
                }

                if (farmVertices.Count > 3)
                {
                    CreateFarm();
                }
            }
        }

        private void CancelConstruction()
        {
            Object.Destroy(pointer);
            Object.Destroy(farmPreview.gameObject);

            farmVertices = new List<Vector3>();
            farmPreview = null;

            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private void RestartConstruction()
        {
            farmVertices = new List<Vector3>();
        }

        private bool TryGetSnappedPosition(out Vector3 snappedPos)
        {
            var snapDistance = 1f;
            var closestDistSqr = float.MaxValue;
            Vector3? closestVertex = null;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            int layerMask = 1 << 16;

            if (!Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask))
            {
                snappedPos = Vector3.zero;
                return false;
            }

            if (farmVertices != null && farmVertices.Count > 0)
            {
                foreach (var vertex in farmVertices)
                {
                    var distSqr = (hit.point - vertex).sqrMagnitude;
                    if (distSqr < closestDistSqr && distSqr <= snapDistance * snapDistance)
                    {
                        closestDistSqr = distSqr;
                        closestVertex = vertex;
                    }
                }
            }

            if (closestVertex.HasValue)
            {
                snappedPos = closestVertex.Value;
            }
            else
            {
                snappedPos = hit.point;
            }

            return true;
        }

        private void CreatePointer()
        {
            pointer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pointer.transform.localScale = new Vector3(1f, .001f, 1f);
            pointer.name = "Pointer";
            pointer.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.yellowGreen };
        }

        private void CreateFarmPreview()
        {
            var farmPreviewGo = new GameObject("FarmPreview");
            farmPreview = farmPreviewGo.AddComponent<FarmPreviewView>();
        }

        private void CreateFarm()
        {
            var farmView = prefabManager.Instantiate<FarmView>("FarmView");
            farmView.Init(buildingDefinition, farmVertices);
            farmView.PlaceBuilding();

            //BlockBuildingArea(farmView.GetComponent<MeshCollider>());

            if (farmView.TryGetComponent(out BuildingView buildingView) && buildingView.EntranceTransform != null)
                ConnectEntranceNode(buildingView.EntranceTransform.position);

            farmView.transform.SetParent(constructionsContainer);

            farmVertices = new List<Vector3>();
            farmPreview.Clear();
        }

        private void ConnectEntranceNode(Vector3 entrancePos)
        {
            var connectRadius = 3f;
            var entranceNode = new Node<Vector3>(
                entrancePos,
                NodeType.Road,
                (a, b) => Vector3.Distance(a.Data, b.Data),
                (a, goal) => Vector3.Distance(a.Data, goal.Data)
            );

            foreach (var node in navigationGraph.Nodes)
            {
                if (node.NodeType == NodeType.Building || node.NodeType == NodeType.Block)
                    continue;

                float distSqr = (node.Data - entrancePos).sqrMagnitude;
                if (distSqr <= connectRadius * connectRadius)
                {
                    if (!entranceNode.Neighbors.Contains(node))
                        entranceNode.Neighbors.Add(node);

                    if (!node.Neighbors.Contains(entranceNode))
                        node.Neighbors.Add(entranceNode);
                }
            }

            navigationGraph.Nodes.Add(entranceNode);
        }

        private void BlockBuildingArea(Collider collider)
        {
            foreach (var node in navigationGraph.Nodes)
            {
                if (node.NodeType == NodeType.Road)
                    continue;

                var pos = node.Data;
                if (IsPointInside(collider, pos))
                {
                    node.NodeType = NodeType.Building;
                    node.Neighbors.Clear();
                }
            }
        }

        private bool IsPointInside(Collider col, Vector3 worldPos)
        {
            Vector3 closest = col.ClosestPoint(worldPos);
            return closest == worldPos;
        }
    }
}