using App.Configs;
using App.Helpers;
using App.Signals;
using Models.Construction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionBuilder<T> : IConstruction where T : BuildingView
    {
        public class Factory : PlaceholderFactory<ConstructionBuilder<T>> { }

        private T building;
        private BuildingDefinition buildingDefinition;

        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;
        private readonly ConstructionDataImporter constructionData;
        private readonly ConstructionConfig constructionConfig;
        private readonly ConstructionGrid constructionGrid;

        private Transform constructionsContainer;
        private Camera mainCamera;
        private readonly Terrain terrain;

        private int rotationSteps;
        private const int samplesPerTile = 3;
        private const int layerMask = 1 << 16;
        private const float raycastDistance = 200f;

        private readonly PointerEventData pointerEventData;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>(8);

        public ConstructionBuilder(SignalBus signalBus, PrefabManager prefabManager, ConstructionDataImporter constructionData,
            ConstructionConfig constructionConfig, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.constructionData = constructionData;
            this.constructionConfig = constructionConfig;
            this.constructionGrid = constructionGrid;

            mainCamera = Camera.main;
            terrain = Terrain.activeTerrain;
            pointerEventData = new PointerEventData(EventSystem.current);
        }

        public void Setup(BuildingDefinition buildingDefinition, Transform constructionsContainer)
        {
            this.buildingDefinition = buildingDefinition;
            this.constructionsContainer = constructionsContainer;
        }

        public void Initialize()
        {
            signalBus.Fire(new ConstructionSignals.ActivateConstructionMode(true));

            building = prefabManager.Instantiate<T>(buildingDefinition.ToString());

            float yRotation = rotationSteps * 90f;
            building.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        public void Tick()
        {
            if (Input.GetMouseButtonUp(1))
            {
                if (building != null)
                {
                    CancelConstruction();
                }
            }

            if (building == null || !GetGridCell(out Vector2Int position))
                return;

            RotateConstruction();
            UpdatePosition(position);

            var occupiedCells = CalculateOccupiedTiles(position);

            if (constructionGrid.IsValidPlacement(occupiedCells) && IsTerrainFlat(position))
            {
                foreach (var renderer in building.GetComponentsInChildren<MeshRenderer>())
                    renderer.material.color = Color.lightGreen;

                if (Input.GetMouseButtonUp(0) && !IsUIHit())
                {
                    constructionGrid.AddOccupant(occupiedCells, TileType.Building, building);
                    PlaceBuilding();
                }
            }
            else
            {
                foreach (var renderer in building.GetComponentsInChildren<MeshRenderer>())
                    renderer.material.color = Color.softRed;
            }
        }

        public void Dispose()
        {
            if (building != null)
                Object.Destroy(building.gameObject);

            signalBus.Fire(new ConstructionSignals.ActivateConstructionMode(false));
        }

        private void CancelConstruction()
        {
            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private void UpdatePosition(Vector2Int position)
        {
            var data = constructionData.ConstructionData[buildingDefinition];
            int normalizedRotation = rotationSteps % 4;

            int rotatedWidth = (normalizedRotation % 2 == 0) ? data.Width : data.Height;
            int rotatedHeight = (normalizedRotation % 2 == 0) ? data.Height : data.Width;

            float offsetX = (rotatedWidth % 2 == 0) ? 0f : 0.5f;
            float offsetZ = (rotatedHeight % 2 == 0) ? 0f : 0.5f;

            float worldX = position.x + offsetX;
            float worldZ = position.y + offsetZ;

            var h = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
            building.transform.position = new Vector3(worldX, h, worldZ);

            if (building.BuildingFoundation != null)
                building.BuildingFoundation.CalculateFoundationGround();
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
            building.transform.SetParent(constructionsContainer);

            building = null;
            signalBus.Fire(new ConstructionSignals.ConstructionMode(buildingDefinition));
        }

        private bool GetGridCell(out Vector2Int cell)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, layerMask))
            {
                cell = default;
                return false;
            }

            int gridX = Mathf.FloorToInt(hit.point.x);
            int gridZ = Mathf.FloorToInt(hit.point.z);

            cell = new Vector2Int(gridX, gridZ);
            return true;
        }

        private bool IsUIHit()
        {
            pointerEventData.position = Input.mousePosition;
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            return raycastResults.Count > 0;
        }

        private List<Vector2Int> CalculateOccupiedTiles(Vector2Int buildingPosition)
        {
            var occupiedTiles = new List<Vector2Int>();

            if (!constructionData.ConstructionData.ContainsKey(buildingDefinition))
            {
                Debug.LogWarning($"No data for the building: {buildingDefinition}");
                return occupiedTiles;
            }

            var data = constructionData.ConstructionData[buildingDefinition];
            int normalizedRotation = rotationSteps % 4;

            int width = (normalizedRotation % 2 == 0) ? data.Width : data.Height;
            int height = (normalizedRotation % 2 == 0) ? data.Height : data.Width;

            int pivotX = width / 2;
            int pivotY = height / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int relX = x - pivotX;
                    int relY = y - pivotY;

                    occupiedTiles.Add(new Vector2Int(
                        buildingPosition.x + relX,
                        buildingPosition.y + relY
                    ));
                }
            }

            return occupiedTiles;
        }

        private bool IsTerrainFlat(Vector2Int buildingPosition)
        {
            if (!constructionData.ConstructionData.ContainsKey(buildingDefinition))
            {
                Debug.LogWarning($"No mask for the building: {buildingDefinition}");
                return false;
            }

            var occupiedTiles = CalculateOccupiedTiles(buildingPosition);

            if (occupiedTiles.Count == 0)
                return true;

            var heights = new List<float>();

            foreach (var tile in occupiedTiles)
            {
                for (int sy = 0; sy < samplesPerTile; sy++)
                {
                    for (int sx = 0; sx < samplesPerTile; sx++)
                    {
                        float sampleX = tile.x + (sx / (float)(samplesPerTile - 1));
                        float sampleZ = tile.y + (sy / (float)(samplesPerTile - 1));

                        float h = terrain.SampleHeight(new Vector3(sampleX, 0, sampleZ));
                        heights.Add(h);
                    }
                }
            }

            float minHeight = heights.Min();
            float maxHeight = heights.Max();
            float heightDifference = maxHeight - minHeight;

            return heightDifference <= constructionConfig.MaxHeightDiff;
        }
    }
}
