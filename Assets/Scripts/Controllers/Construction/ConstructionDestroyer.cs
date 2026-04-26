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

        public ConstructionDestroyer(SignalBus signalBus, ConstructionGrid constructionGrid)
        {
            this.signalBus = signalBus;
            this.constructionGrid = constructionGrid;

            mainCamera = Camera.main;
        }

        public void Initialize()
        {

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
            }

            if (Input.GetMouseButton(0) && isSelecting)
            {
                endPoint = cell;
                UpdateSelection();
            }

            if (Input.GetMouseButtonUp(0) && isSelecting)
            {
                endPoint = cell;
                UpdateSelection();
                isSelecting = false;

                foreach (var tilePos in selectedTiles)
                {
                    var buildingsToDestroy = constructionGrid.GetTileByPosition(tilePos);
                    if (buildingsToDestroy != null)
                    {
                        constructionGrid.RemoveOccupant(tilePos);
                        buildingsToDestroy.BuildingView.DestroyBuilding();
                        Object.Destroy(buildingsToDestroy.BuildingView.gameObject);
                    }
                }
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