using App.Signals;
using Models.Construction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionDestroyer : IConstruction
    {
        public class Factory : PlaceholderFactory<ConstructionDestroyer> { }

        private SignalBus signalBus;
        private ConstructionGrid constructionGrid;
        private Camera mainCamera;

        private const int layerMask = 1 << 16;
        private const float raycastDistance = 100f;

        private Vector2Int startPoint;
        private Vector2Int endPoint;
        private List<Vector2Int> selectedTiles = new List<Vector2Int>();
        private bool isSelecting;

        private Vector2Int? lastHighlightCell;
        private HashSet<Vector2Int> lastSelectedTiles = new HashSet<Vector2Int>();

        public ConstructionDestroyer(SignalBus signalBus, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.constructionGrid = constructionGrid;

            mainCamera = Camera.main;
        }

        public void Initialize()
        {
            signalBus.Fire(new ConstructionSignals.ActivateConstructionMode(true));
        }

        public void Tick()
        {
            if (Input.GetMouseButtonUp(1))
            {
                isSelecting = false;
                signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
            }

            if (!TryGetGridCell(out var cell) || IsUIHit())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                startPoint = cell;
                endPoint = cell;
                isSelecting = true;

                lastHighlightCell = null;
                lastSelectedTiles.Clear();
            }

            if (Input.GetMouseButton(0) && isSelecting)
            {
                if (!lastHighlightCell.HasValue || lastHighlightCell.Value != cell)
                {
                    endPoint = cell;
                    UpdateSelection();

                    foreach (var oldTile in lastSelectedTiles)
                    {
                        if (!selectedTiles.Contains(oldTile))
                        {
                            var building = constructionGrid.GetTileByPosition(oldTile);
                            building?.BuildingView.Highlight(false, default);
                        }
                    }

                    foreach (var tilePos in selectedTiles)
                    {
                        if (!lastSelectedTiles.Contains(tilePos))
                        {
                            var building = constructionGrid.GetTileByPosition(tilePos);
                            building?.BuildingView.Highlight(true, Color.darkRed);
                        }
                    }

                    lastSelectedTiles.Clear();
                    foreach (var t in selectedTiles)
                        lastSelectedTiles.Add(t);

                    lastHighlightCell = cell;
                }
            }

            if (Input.GetMouseButtonUp(0) && isSelecting)
            {
                endPoint = cell;
                UpdateSelection();
                isSelecting = false;

                foreach (var tilePos in selectedTiles)
                {
                    var building = constructionGrid.GetTileByPosition(tilePos);
                    if (building != null)
                    {
                        constructionGrid.RemoveOccupant(tilePos);
                        building.BuildingView.DestroyBuilding();
                        Object.Destroy(building.BuildingView.gameObject);
                    }
                }

                lastSelectedTiles.Clear();
            }
        }

        private void UpdateSelection()
        {
            selectedTiles.Clear();

            var minX = Mathf.Min(startPoint.x, endPoint.x);
            var maxX = Mathf.Max(startPoint.x, endPoint.x);

            var minY = Mathf.Min(startPoint.y, endPoint.y);
            var maxY = Mathf.Max(startPoint.y, endPoint.y);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    selectedTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        public void Dispose()
        {
            signalBus.Fire(new ConstructionSignals.ActivateConstructionMode(false));

            startPoint = default;
            endPoint = default;
            selectedTiles = null;
        }

        private bool TryGetGridCell(out Vector2Int cell)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, layerMask))
            {
                cell = default;
                return false;
            }

            var gridX = Mathf.FloorToInt(hit.point.x);
            var gridZ = Mathf.FloorToInt(hit.point.z);

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
    }
}