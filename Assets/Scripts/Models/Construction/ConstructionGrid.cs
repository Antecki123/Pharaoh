using Controllers.Construction;
using System;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;

namespace Models.Construction
{
    public class ConstructionGrid
    {
        public event Action OnValueChanged;

        public IReadOnlyCollection<ConstructionGridData> OccupiedTiles => occupiedTiles;
        public IReadOnlyCollection<Vector2Int> OccupiedTilesWithoutRoads => occupiedTilesWithoutRoads;
        public IReadOnlyCollection<Vector2Int> RoadsTiles => roadsTiles;

        private HashSet<ConstructionGridData> occupiedTiles = new HashSet<ConstructionGridData>();
        private HashSet<Vector2Int> occupiedTilesWithoutRoads = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> roadsTiles = new HashSet<Vector2Int>();

        public void AddOccupant(List<Vector2Int> cells, BuildingDefinition buildingDefinition, BuildingView buildingView)
        {
            foreach (var pos in cells)
            {
                var data = new ConstructionGridData()
                {
                    Position = pos,
                    BuildingDefinition = buildingDefinition,
                    BuildingView = buildingView
                };
                occupiedTiles.Add(data);

                if (buildingDefinition != BuildingDefinition.Road)
                    occupiedTilesWithoutRoads.Add(pos);
                else
                    roadsTiles.Add(pos);
            }

            OnValueChanged?.Invoke();
        }

        public void AddOccupant(Vector2Int pos, BuildingDefinition buildingDefinition, BuildingView buildingView)
        {
            var data = new ConstructionGridData()
            {
                Position = pos,
                BuildingDefinition = buildingDefinition,
                BuildingView = buildingView
            };
            occupiedTiles.Add(data);

            if (buildingDefinition != BuildingDefinition.Road)
                occupiedTilesWithoutRoads.Add(pos);
            else
                roadsTiles.Add(pos);

            OnValueChanged?.Invoke();
        }

        public void RemoveOccupant(Vector2Int cellToRemove)
        {
            if (occupiedTilesWithoutRoads.Contains(cellToRemove))
                occupiedTilesWithoutRoads.Remove(cellToRemove);

            if (roadsTiles.Contains(cellToRemove))
                roadsTiles.Remove(cellToRemove);

            OnValueChanged.Invoke();
        }

        public bool IsValidPlacement(List<Vector2Int> cells)
        {
            foreach (var cell in cells)
            {
                foreach (var tile in occupiedTiles)
                {
                    if (cell == tile.Position)
                        return false;
                }
            }

            return true;
        }

        public bool HasRoadConnection(BuildingView buildingView)
        {
            foreach (var tile in occupiedTiles)
            {
                if (tile.BuildingView != buildingView)
                    continue;

                var pos = tile.Position;
                Vector2Int[] neighbours =
                {
                    pos + Vector2Int.up,
                    pos + Vector2Int.down,
                    pos + Vector2Int.left,
                    pos + Vector2Int.right
                };

                foreach (var neighbourPos in neighbours)
                {
                    var neighbourData = GetTileByPosition(neighbourPos);

                    if (neighbourData != null && neighbourData.BuildingDefinition == BuildingDefinition.Road)
                        return true;
                }
            }

            return false;
        }

        public List<Vector2Int> GetAllConnectedRoadTiles(BuildingView buildingView)
        {
            var roadTiles = new List<Vector2Int>();
            foreach (var tile in occupiedTiles)
            {
                if (tile.BuildingView != buildingView)
                    continue;

                var pos = tile.Position;
                Vector2Int[] neighbours =
                {
                    pos + Vector2Int.up,
                    pos + Vector2Int.down,
                    pos + Vector2Int.left,
                    pos + Vector2Int.right
                };

                foreach (var neighbourPos in neighbours)
                {
                    var lookup = new ConstructionGridData { Position = neighbourPos };

                    if (!occupiedTiles.TryGetValue(lookup, out var neighbourTile))
                        continue;

                    if (neighbourTile.BuildingDefinition == BuildingDefinition.Road)
                    {
                        if (!roadTiles.Contains(neighbourPos))
                            roadTiles.Add(neighbourPos);
                    }
                }
            }

            return roadTiles;
        }

        public ConstructionGridData GetTileByPosition(Vector2Int position)
        {
            foreach (var tile in occupiedTiles)
            {
                if (tile.Position == position)
                    return tile;
            }

            return null;
        }
    }

    public class ConstructionGridData
    {
        public Vector2Int Position { get; set; }

        public BuildingDefinition BuildingDefinition { get; set; }

        public BuildingView BuildingView { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not ConstructionGridData other)
                return false;

            return Position.Equals(other.Position);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
