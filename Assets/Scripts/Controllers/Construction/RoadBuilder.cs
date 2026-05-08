using App.Helpers;
using App.Signals;
using Models.Construction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Views.Road;
using Zenject;

namespace Controllers.Construction
{
    public class RoadBuilder : IConstruction
    {
        public class Factory : PlaceholderFactory<RoadBuilder> { }

        private readonly Stack<RoadView> poolInactive = new Stack<RoadView>(32);
        private readonly List<RoadView> poolActive = new List<RoadView>(32);
        private readonly List<Vector2Int> activePositions = new List<Vector2Int>(32);
        private readonly List<Vector2Int> currentRoadPath = new List<Vector2Int>(64);

        private Vector2Int? startPosition;
        private Vector2Int? endPosition;
        private Vector2Int? lastCell;

        private Camera mainCamera;
        private readonly Terrain terrain;
        private readonly RoadPathfinder roadPathfinder;

        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;
        private readonly ConstructionGrid constructionGrid;

        private readonly PointerEventData pointerEventData;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>(8);

        private Transform roadContainer;

        private const int layerMask = 1 << 16;
        private const float raycastDistance = 200f;
        private const float cellOffset = 0.5f;

        public RoadBuilder(SignalBus signalBus, PrefabManager prefabManager, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.constructionGrid = constructionGrid;

            mainCamera = Camera.main;
            terrain = Terrain.activeTerrain;
            roadPathfinder = new RoadPathfinder(constructionGrid.OccupiedTilesWithoutRoads);
            pointerEventData = new PointerEventData(EventSystem.current);
        }

        public void Setup(Transform roadContainer)
        {
            this.roadContainer = roadContainer;
        }

        public void Initialize()
        {
            lastCell = null;
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (startPosition != null)
                    RestartConstruction();
                else
                    CancelConstruction();
            }

            if (!GetGridCell(out Vector2Int currentCell))
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (!constructionGrid.IsValidPlacement(currentCell, true))
                    return;

                if (startPosition == null)
                {
                    startPosition = currentCell;
                    lastCell = null;
                }
                else
                {
                    if (constructionGrid.IsValidPlacement(currentCell, true) && !IsUIHit())
                        ConfirmRoadConstruction();
                }
            }

            if (startPosition != null && endPosition == null)
            {
                if (lastCell == null || lastCell.Value != currentCell)
                {
                    UpdateRoadPreview(currentCell);
                    lastCell = currentCell;
                }
            }
        }

        private void UpdateRoadPreview(Vector2Int currentCell)
        {
            constructionGrid.ClearRoadPreview();
            ReturnAllToPool();

            roadPathfinder.FindRoadPath(startPosition.Value, currentCell, currentRoadPath);

            bool isValid = IsValidPlacement();
            Color previewColor = isValid ? Color.lightGreen : Color.softRed;

            for (int i = 0; i < currentRoadPath.Count; i++)
            {
                var roadPosition = currentRoadPath[i];
                if (constructionGrid.RoadsTiles.Contains(roadPosition))
                    continue;

                var worldX = roadPosition.x + cellOffset;
                var worldZ = roadPosition.y + cellOffset;
                var height = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));

                var view = RentFromPool();
                view.transform.position = new Vector3(worldX, height, worldZ);
                view.CreatePreview(roadPosition);
                view.SetColor(previewColor);

                activePositions.Add(roadPosition);
                constructionGrid.AddRoadPreview(roadPosition);
            }
        }

        public void Dispose()
        {
            startPosition = null;
            endPosition = null;
            currentRoadPath.Clear();
        }

        private void ConfirmRoadConstruction()
        {
            for (int i = 0; i < poolActive.Count; i++)
            {
                constructionGrid.AddOccupant(activePositions[i], TileType.Road, poolActive[i]);
                poolActive[i].PlaceBuilding();
            }

            poolActive.Clear();
            activePositions.Clear();

            startPosition = null;
            endPosition = null;
            currentRoadPath.Clear();
        }

        private void RestartConstruction()
        {
            constructionGrid.ClearRoadPreview();
            ReturnAllToPool();

            startPosition = null;
            endPosition = null;
            currentRoadPath.Clear();
        }

        private void CancelConstruction()
        {
            constructionGrid.ClearRoadPreview();
            ReturnAllToPool();

            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private RoadView RentFromPool()
        {
            RoadView view;
            if (poolInactive.Count > 0)
            {
                view = poolInactive.Pop();
                view.gameObject.SetActive(true);
            }
            else
            {
                view = prefabManager.Instantiate<RoadView>("Road");
                view.transform.SetParent(roadContainer);
            }
            poolActive.Add(view);
            return view;
        }

        private void ReturnAllToPool()
        {
            for (int i = 0; i < poolActive.Count; i++)
            {
                poolActive[i].gameObject.SetActive(false);
                poolInactive.Push(poolActive[i]);
            }
            poolActive.Clear();
            activePositions.Clear();
        }

        private bool GetGridCell(out Vector2Int cell)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, layerMask) || IsUIHit())
            {
                cell = default;
                return false;
            }

            cell = new Vector2Int(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.z));
            return true;
        }

        private bool IsUIHit()
        {
            pointerEventData.position = Input.mousePosition;
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            return raycastResults.Count > 0;
        }

        private bool IsValidPlacement()
        {
            for (int i = 0; i < currentRoadPath.Count; i++)
            {
                if (!constructionGrid.IsValidPlacement(currentRoadPath[i], true))
                    return false;
            }
            return true;
        }
    }
}