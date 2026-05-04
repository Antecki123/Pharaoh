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

        private List<ConstructingRoadPresenter> constructingRoads = new List<ConstructingRoadPresenter>();
        private List<Vector2Int> currentRoadPath = new List<Vector2Int>();
        private Vector2Int? startPosition;
        private Vector2Int? endPosition;
        private Vector2Int? lastCell;

        private Camera mainCamera;
        private RoadPathfinder roadPathfinder;

        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;
        private readonly ConstructionGrid constructionGrid;

        private Transform roadContainer;

        private const int layerMask = 1 << 16;
        private const float raycastDistance = 100f;
        private const float cellOffset = 0.5f;

        public RoadBuilder(SignalBus signalBus, PrefabManager prefabManager, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.constructionGrid = constructionGrid;

            mainCamera = Camera.main;
            roadPathfinder = new RoadPathfinder(constructionGrid.OccupiedTilesWithoutRoads);
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
                {
                    RestartConstruction();
                }
                else
                {
                    CancelConstruction();
                }
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
                    constructionGrid.ClearRoadPreview();

                    currentRoadPath = roadPathfinder.FindRoadPath(startPosition.Value, currentCell);
                    lastCell = currentCell;

                    constructingRoads.ForEach(x => Object.Destroy(x.View.gameObject));
                    constructingRoads.Clear();

                    foreach (var roadPosition in currentRoadPath)
                    {
                        if (constructionGrid.RoadsTiles.Contains(roadPosition))
                            continue;

                        var worldX = roadPosition.x + cellOffset;
                        var worldZ = roadPosition.y + cellOffset;
                        var height = Terrain.activeTerrain.SampleHeight(new Vector3(worldX, 0, worldZ));

                        var newRoad = prefabManager.Instantiate<RoadView>("Road");
                        newRoad.transform.position = new Vector3(worldX, height, worldZ);
                        newRoad.transform.SetParent(roadContainer);
                        newRoad.CreatePreview(roadPosition);

                        constructingRoads.Add(new ConstructingRoadPresenter()
                        {
                            View = newRoad,
                            Position = roadPosition
                        });
                        constructionGrid.AddRoadPreview(roadPosition);
                    }

                    foreach (var road in constructingRoads)
                    {
                        var color = IsValidPlacement() ? Color.lightGreen : Color.softRed;
                        road.View.SetColor(color);
                    }
                }
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
            foreach (var road in constructingRoads)
            {
                constructionGrid.AddOccupant(road.Position, BuildingDefinition.Road, road.View);
                road.View.PlaceBuilding();
            }

            startPosition = null;
            endPosition = null;
            currentRoadPath.Clear();
            constructingRoads.Clear();
        }

        private void RestartConstruction()
        {
            constructionGrid.ClearRoadPreview();

            constructingRoads.ForEach(x => Object.Destroy(x.View.gameObject));
            constructingRoads.Clear();

            startPosition = null;
            endPosition = null;
            currentRoadPath.Clear();
        }

        private void CancelConstruction()
        {
            constructionGrid.ClearRoadPreview();

            constructingRoads.ForEach(x => Object.Destroy(x.View.gameObject));
            constructingRoads.Clear();

            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
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

            int gridX = Mathf.FloorToInt(hit.point.x);
            int gridZ = Mathf.FloorToInt(hit.point.z);

            cell = new Vector2Int(gridX, gridZ);
            return true;
        }

        private bool IsUIHit()
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        private bool IsValidPlacement()
        {
            foreach (var roadPosition in currentRoadPath)
            {
                if (!constructionGrid.IsValidPlacement(roadPosition, true))
                    return false;
            }

            return true;
        }
    }

    public class ConstructingRoadPresenter
    {
        public RoadView View { get; set; }

        public Vector2Int Position { get; set; }
    }
}