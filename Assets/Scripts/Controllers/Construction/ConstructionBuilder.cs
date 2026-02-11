using App.Configs;
using App.Helpers;
using App.Signals;
using Models.Ai;
using Models.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionBuilder<T> : IConstruction where T : BuildingView
    {
        public class Factory : PlaceholderFactory<ConstructionBuilder<T>> { }

        private T building;
        private BuildingDefinition buildingDefinition;

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private NavigationGraph navigationGraph;
        private ConstructionConfig constructionConfig;
        private ConstructionDataImporter constructionData;
        private ConstructionGrid constructionGrid;

        private Transform constructionsContainer;
        private Camera mainCamera;

        private readonly float cellSize = 4f;
        private int rotationSteps;

        public ConstructionBuilder(SignalBus signalBus, PrefabManager prefabManager, NavigationGraph navigationGraph, ConstructionConfig constructionConfig,
            ConstructionDataImporter constructionData, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.navigationGraph = navigationGraph;
            this.constructionConfig = constructionConfig;
            this.constructionData = constructionData;
            this.constructionGrid = constructionGrid;

            mainCamera = Camera.main;
        }

        public void Setup(BuildingDefinition buildingDefinition, Transform constructionsContainer)
        {
            this.buildingDefinition = buildingDefinition;
            this.constructionsContainer = constructionsContainer;
        }

        public void Initialize()
        {
            building = prefabManager.Instantiate<T>(buildingDefinition.ToString());

            float yRotation = rotationSteps * 90f;
            building.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
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

            if (building == null || !GetGridCell(out Vector2Int position))
                return;

            UpdatePosition(position);
            RotateConstruction();

            var occupiedCells = CalculateOccupiedTiles(position);

            if (Input.GetMouseButtonDown(0) && constructionGrid.IsValidPlacement(occupiedCells) && IsTerrainFlat(position))
            {
                var buildingId = Guid.NewGuid();
                constructionGrid.AddOccupant(occupiedCells, buildingDefinition, building);

                PlaceBuilding();
            }
        }

        private void CancelConstruction()
        {
            if (building != null)
                UnityEngine.Object.Destroy(building.gameObject);

            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private void UpdatePosition(Vector2Int position)
        {
            int[,] mask = ConstructionFootprintMasks.ConstructionFootprintMask[buildingDefinition];
            int height = mask.GetLength(0);
            int width = mask.GetLength(1);
            int normalizedRotation = rotationSteps % 4;

            int rotatedWidth = (normalizedRotation % 2 == 0) ? width : height;
            int rotatedHeight = (normalizedRotation % 2 == 0) ? height : width;

            float offsetX = (rotatedWidth % 2 == 1) ? 0.5f : 0f;
            float offsetZ = (rotatedHeight % 2 == 1) ? 0.5f : 0f;

            float worldX = (position.x + offsetX) * cellSize;
            float worldZ = (position.y + offsetZ) * cellSize;

            var h = Terrain.activeTerrain.SampleHeight(new Vector3(worldX, 0, worldZ));
            building.transform.position = new Vector3(worldX, h, worldZ);
        }

        private void RotateConstruction()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rotationSteps = (rotationSteps + 1) % 4;
                building.transform.rotation = Quaternion.Euler(0f, rotationSteps * 90f, 0f);
            }
        }

        private void PlaceBuilding()
        {
            foreach (var renderer in building.GetComponentsInChildren<MeshRenderer>())
                renderer.material.color = Color.white;

            building.PlaceBuilding();

            //if (building.TryGetComponent(out BuildingView buildingView) && buildingView.EntranceTransform != null)
            //    ConnectEntranceNode(buildingView.EntranceTransform.position);

            building.transform.SetParent(constructionsContainer);

            building = null;
            signalBus.Fire(new ConstructionSignals.ConstructionMode(buildingDefinition));
        }

        private bool GetGridCell(out Vector2Int cell)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var layerMask = 1 << 16;

            if (!Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask))
            {
                cell = default;
                return false;
            }

            int gridX = Mathf.FloorToInt(hit.point.x / cellSize);
            int gridZ = Mathf.FloorToInt(hit.point.z / cellSize);

            cell = new Vector2Int(gridX, gridZ);
            return true;
        }

        private List<Vector2Int> CalculateOccupiedTiles(Vector2Int buildingPosition)
        {
            var occupiedTiles = new List<Vector2Int>();
            if (!ConstructionFootprintMasks.ConstructionFootprintMask.ContainsKey(buildingDefinition))
            {
                Debug.LogWarning($"No mask for the building: {buildingDefinition}");
                return occupiedTiles;
            }

            int[,] mask = ConstructionFootprintMasks.ConstructionFootprintMask[buildingDefinition];
            int height = mask.GetLength(0);
            int width = mask.GetLength(1);
            int normalizedRotation = rotationSteps % 4;
            int pivotX = width / 2;
            int pivotY = height / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[y, x] != 0)
                    {
                        int relX = x - pivotX;
                        int relY = y - pivotY;

                        Vector2Int rotatedPos = normalizedRotation switch
                        {
                            0 => new Vector2Int(relX, relY),
                            1 => new Vector2Int(-relY - (height % 2 == 0 ? 1 : 0), relX),
                            2 => new Vector2Int(-relX - (width % 2 == 0 ? 1 : 0), -relY - (height % 2 == 0 ? 1 : 0)),
                            3 => new Vector2Int(relY, -relX - (width % 2 == 0 ? 1 : 0)),
                            _ => new Vector2Int(relX, relY),
                        };

                        var tilePosition = new Vector2Int(
                            buildingPosition.x + rotatedPos.x,
                            buildingPosition.y + rotatedPos.y
                        );
                        occupiedTiles.Add(tilePosition);
                    }
                }
            }
            return occupiedTiles;
        }

        private bool IsTerrainFlat(Vector2Int buildingPosition)
        {
            if (!ConstructionFootprintMasks.ConstructionFootprintMask.ContainsKey(buildingDefinition))
            {
                Debug.LogWarning($"No mask for the building: {buildingDefinition}");
                return false;
            }

            int[,] mask = ConstructionFootprintMasks.ConstructionFootprintMask[buildingDefinition];
            int height = mask.GetLength(0);
            int width = mask.GetLength(1);
            int normalizedRotation = rotationSteps % 4;

            int rotatedWidth = (normalizedRotation % 2 == 0) ? width : height;
            int rotatedHeight = (normalizedRotation % 2 == 0) ? height : width;

            var occupiedTiles = CalculateOccupiedTiles(buildingPosition);

            if (occupiedTiles.Count == 0)
                return true;

            int samplesPerTile = 3;
            var heights = new List<float>();

            foreach (var tile in occupiedTiles)
            {
                for (int sy = 0; sy < samplesPerTile; sy++)
                {
                    for (int sx = 0; sx < samplesPerTile; sx++)
                    {
                        float sampleX = tile.x * cellSize + (sx / (float)(samplesPerTile - 1)) * cellSize;
                        float sampleZ = tile.y * cellSize + (sy / (float)(samplesPerTile - 1)) * cellSize;

                        float h = Terrain.activeTerrain.SampleHeight(new Vector3(sampleX, 0, sampleZ));
                        heights.Add(h);
                    }
                }
            }

            float minHeight = heights.Min();
            float maxHeight = heights.Max();
            float heightDifference = maxHeight - minHeight;

            return heightDifference <= constructionConfig.MaxHeightDiff;
        }

        /*private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }*/

        /*private bool IsAvailableSpace(Vector3 position)
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
        }*/

        /*private void BlockBuildingArea(Collider collider)
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
        }*/

        /*private bool IsPointInside(Collider col, Vector3 worldPos)
        {
            Vector3 closest = col.ClosestPoint(worldPos);
            return closest == worldPos;
        }*/

        /*private void ConnectEntranceNode(Vector3 entrancePos)
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
        }*/
    }
}
