using App.Helpers;
using App.Signals;
using Controllers.Application;
using Models.Construction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionDestroyer : IConstruction
    {
        public class Factory : PlaceholderFactory<ConstructionDestroyer> { }

        private readonly SignalBus signalBus;
        private readonly ConstructionGrid constructionGrid;
        private readonly PrefabManager prefabManager;
        private Camera mainCamera;

        private const int layerMask = 1 << 16;
        private const float raycastDistance = 200f;

        private bool isSelecting;
        private Vector2Int? lastHighlightCell;
        private Vector2Int startPoint;
        private Vector2Int endPoint;

        private readonly List<Vector2Int> selectedTiles = new List<Vector2Int>(64);
        private readonly HashSet<Vector2Int> lastSelectedTiles = new HashSet<Vector2Int>(64);
        private readonly HashSet<BuildingView> currentViews = new HashSet<BuildingView>(32);
        private readonly HashSet<BuildingView> previousViews = new HashSet<BuildingView>(32);

        private SelectionMaskView selectionMaskView;

        private readonly PointerEventData pointerEventData;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>(8);

        public ConstructionDestroyer(SignalBus signalBus, ConstructionGrid constructionGrid, PrefabManager prefabManager)
        {
            this.signalBus = signalBus;
            this.constructionGrid = constructionGrid;
            this.prefabManager = prefabManager;

            mainCamera = Camera.main;
            pointerEventData = new PointerEventData(EventSystem.current);
        }

        public void Initialize()
        {
            signalBus.Fire(new ConstructionSignals.ActivateConstructionMode(true));
            signalBus.Fire(new ApplicationSignals.SetCursor(CursorState.Impossible));
            selectionMaskView = prefabManager.Instantiate<SelectionMaskView>("SelectionMaskView");
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

            if (!isSelecting)
            {
                selectionMaskView.UpdateMask(cell, cell);
            }

            if (Input.GetMouseButtonDown(0))
            {
                startPoint = cell;
                endPoint = cell;
                isSelecting = true;
                lastHighlightCell = null;
                lastSelectedTiles.Clear();
            }

            if (selectionMaskView != null && isSelecting)
            {
                selectionMaskView.UpdateMask(startPoint, endPoint);
            }

            if (Input.GetMouseButton(0) && isSelecting)
            {
                if (!lastHighlightCell.HasValue || lastHighlightCell.Value != cell)
                {
                    endPoint = cell;
                    UpdateSelection();
                    UpdateHighlights();
                    lastHighlightCell = cell;
                }
            }

            if (Input.GetMouseButtonUp(0) && isSelecting)
            {
                endPoint = cell;
                UpdateSelection();
                isSelecting = false;
                DestroySelected();
                lastSelectedTiles.Clear();
            }
        }

        private void UpdateHighlights()
        {
            currentViews.Clear();
            previousViews.Clear();

            for (int i = 0; i < selectedTiles.Count; i++)
            {
                var b = constructionGrid.GetTileByPosition(selectedTiles[i]);
                if (b?.BuildingView != null)
                    currentViews.Add(b.BuildingView);
            }

            foreach (var tilePos in lastSelectedTiles)
            {
                var b = constructionGrid.GetTileByPosition(tilePos);
                if (b?.BuildingView != null)
                    previousViews.Add(b.BuildingView);
            }

            foreach (var view in currentViews)
                view.Highlight(true, Color.darkRed);

            foreach (var view in previousViews)
            {
                if (!currentViews.Contains(view))
                    view.Highlight(false, default);
            }

            lastSelectedTiles.Clear();
            lastSelectedTiles.UnionWith(selectedTiles);
        }

        private void DestroySelected()
        {
            for (int i = 0; i < selectedTiles.Count; i++)
            {
                var building = constructionGrid.GetTileByPosition(selectedTiles[i]);
                if (building?.BuildingView != null)
                {
                    constructionGrid.RemoveOccupant(selectedTiles[i]);
                    building.BuildingView.DestroyBuilding();
                    Object.Destroy(building.BuildingView.gameObject);
                }
            }
        }

        private void UpdateSelection()
        {
            selectedTiles.Clear();

            int minX = Mathf.Min(startPoint.x, endPoint.x);
            int maxX = Mathf.Max(startPoint.x, endPoint.x);
            int minY = Mathf.Min(startPoint.y, endPoint.y);
            int maxY = Mathf.Max(startPoint.y, endPoint.y);

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    selectedTiles.Add(new Vector2Int(x, y));
        }

        public void Dispose()
        {
            signalBus.Fire(new ConstructionSignals.ActivateConstructionMode(false));
            signalBus.Fire(new ApplicationSignals.SetCursor(CursorState.Default));
            Object.Destroy(selectionMaskView.gameObject);

            startPoint = default;
            endPoint = default;
            selectedTiles.Clear();
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
    }
}