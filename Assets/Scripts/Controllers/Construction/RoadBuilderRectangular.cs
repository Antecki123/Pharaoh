using App.Helpers;
using App.Signals;
using Models.Construction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Views.Road;
using Zenject;

namespace Controllers.Construction
{
    public class RoadBuilderRectangular : IConstruction
    {
        public class Factory : PlaceholderFactory<RoadBuilderRectangular> { }

        private List<Vector2Int> currentRoadPath = new List<Vector2Int>();
        private Vector3? startPosition;
        private Vector3? endPosition;

        private Camera mainCamera;
        private RoadPathfinder roadPathfinder;

        private LineRenderer roadPreview;

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private ConstructionGrid constructionGrid;

        private Transform roadContainer;

        public RoadBuilderRectangular(SignalBus signalBus, PrefabManager prefabManager, ConstructionGrid constructionGrid)
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
            var roadPreviewObject = new GameObject("RoadPreview");
            roadPreview = roadPreviewObject.AddComponent<LineRenderer>();
            roadPreview.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            roadPreview.startWidth = .5f;
            roadPreview.endWidth = .5f;
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (startPosition != null)
                {
                    startPosition = null;
                    endPosition = null;
                    currentRoadPath.Clear();

                    roadPreview.positionCount = 0;
                }
                else
                {
                    CancelConstruction();

                    roadPreview.positionCount = 0;
                }
            }

            if (!GetGridCell(out Vector2Int currentCell))
                return;

            roadPreview.material.color = constructionGrid.IsValidPlacement(currentCell, true)
                ? Color.darkOliveGreen
                : Color.indianRed;

            if (Input.GetMouseButtonDown(0))
            {
                if (!constructionGrid.IsValidPlacement(currentCell, true))
                    return;

                if (startPosition == null)
                {
                    var cellOffset = 0.5f;
                    var worldX = currentCell.x + cellOffset;
                    var worldZ = currentCell.y + cellOffset;
                    var height = Terrain.activeTerrain.SampleHeight(new Vector3(worldX, 0, worldZ));
                    startPosition = new Vector3(worldX, height, worldZ);
                }
                else
                {
                    if (!constructionGrid.IsValidPlacement(currentCell, true))
                        return;

                    ConfirmRoadConstruction();
                    roadPreview.positionCount = 0;
                }
            }

            if (startPosition != null && endPosition == null)
            {
                var startPos = new Vector2Int(Mathf.FloorToInt(startPosition.Value.x), Mathf.FloorToInt(startPosition.Value.z));
                currentRoadPath = roadPathfinder.FindRoadPath(startPos, currentCell);
            }

            if (currentRoadPath.Count != 0)
            {
                var roadPoints = new List<Vector3>();
                foreach (var point in currentRoadPath)
                {
                    var cellOffset = 0.5f;
                    var worldX = point.x + cellOffset;
                    var worldZ = point.y + cellOffset;
                    var height = Terrain.activeTerrain.SampleHeight(new Vector3(worldX, 0, worldZ));
                    roadPoints.Add(new Vector3(worldX, height, worldZ));
                }

                roadPreview.positionCount = currentRoadPath.Count;
                roadPreview.SetPositions(roadPoints.ToArray());
            }
        }

        public void Dispose()
        {
            Object.Destroy(roadPreview.gameObject);

            startPosition = null;
            endPosition = null;
            currentRoadPath.Clear();
        }

        private void ConfirmRoadConstruction()
        {
            var cellOffset = 0.5f;

            foreach (var gridPosition in currentRoadPath)
            {
                var routePrefab = Resources.Load<RoadView>("Prefabs/RoadView");
                var roadView = prefabManager.InstantiateWithInject<RoadView>(routePrefab.gameObject);
                var worldX = gridPosition.x + cellOffset;
                var worldZ = gridPosition.y + cellOffset;
                var height = Terrain.activeTerrain.SampleHeight(new Vector3(worldX, 0, worldZ));

                roadView.Init(new Vector3(worldX, height, worldZ));
                roadView.transform.parent = roadContainer;

                constructionGrid.AddOccupant(gridPosition, BuildingDefinition.Road, roadView);
            }

            startPosition = null;
            endPosition = null;
            currentRoadPath.Clear();
        }

        private void CancelConstruction()
        {
            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private bool GetGridCell(out Vector2Int cell)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var layerMask = 1 << 16;

            if (!Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask) || IsUIHit())
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
    }

    public class RoadPathfinder
    {
        private IReadOnlyCollection<Vector2Int> occupied;

        public RoadPathfinder(IReadOnlyCollection<Vector2Int> occupied)
        {
            this.occupied = occupied;
        }

        public List<Vector2Int> FindRoadPath(Vector2Int startPosition, Vector2Int endPosition)
        {
            if (startPosition == endPosition)
            {
                return new List<Vector2Int> { startPosition };
            }

            var closedSet = new HashSet<Vector2Int>();
            var openSet = new HashSet<Vector2Int> { startPosition };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float>();
            var fScore = new Dictionary<Vector2Int, float>();

            gScore[startPosition] = 0;
            fScore[startPosition] = HeuristicCost(startPosition, endPosition);

            while (openSet.Count > 0)
            {
                Vector2Int current = openSet.OrderBy(pos => fScore.GetValueOrDefault(pos, float.MaxValue)).First();

                if (current == endPosition)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (Vector2Int neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    float tentativeGScore = gScore[current] + GetMovementCost(current, neighbor);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCost(neighbor, endPosition);
                }
            }

            return new List<Vector2Int>();
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private float HeuristicCost(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private float GetMovementCost(Vector2Int from, Vector2Int to)
        {
            if (occupied.Contains(to))
            {
                return 10f;
            }
            return 1f;
        }

        private List<Vector2Int> GetNeighbors(Vector2Int position)
        {
            return new List<Vector2Int>
            {
                position + Vector2Int.up,
                position + Vector2Int.down,
                position + Vector2Int.left,
                position + Vector2Int.right
            };
        }
    }
}