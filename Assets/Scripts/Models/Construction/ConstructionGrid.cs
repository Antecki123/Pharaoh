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

        public event Action<Vector2Int> OnRoadChanged;

        public HashSet<ConstructionGridData> OccupiedTiles => occupiedTiles;
        public HashSet<Vector2Int> OccupiedTilesWithoutRoads => occupiedTilesWithoutRoads;
        public HashSet<Vector2Int> RoadsTiles => roadsTiles;
        public HashSet<Vector2Int> RoadPreview => roadPreview;

        private HashSet<ConstructionGridData> occupiedTiles = new HashSet<ConstructionGridData>();
        private HashSet<Vector2Int> occupiedTilesWithoutRoads = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> roadsTiles = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> roadPreview = new HashSet<Vector2Int>();

        private readonly List<ConstructionGridData> buffer = new List<ConstructionGridData>();

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

            if (buildingDefinition == BuildingDefinition.Road)
                OnRoadChanged?.Invoke(pos);
        }

        public void RemoveOccupant(Vector2Int cellToRemove)
        {
            var tile = GetTileByPosition(cellToRemove);

            if (tile == null)
                return;

            var building = tile.BuildingView;

            if (building == null)
                return;

            buffer.Clear();

            foreach (var t in OccupiedTiles)
            {
                if (t.BuildingView == building)
                {
                    buffer.Add(t);
                }
            }

            foreach (var t in buffer)
            {
                OccupiedTiles.Remove(t);
                occupiedTilesWithoutRoads.Remove(t.Position);
                roadsTiles.Remove(t.Position);
            }

            OnValueChanged?.Invoke();

            if (tile.BuildingDefinition == BuildingDefinition.Road)
                OnRoadChanged?.Invoke(cellToRemove);
        }

        public void AddRoadPreview(Vector2Int pos)
        {
            if (roadsTiles.Contains(pos))
                return;

            roadPreview.Add(pos);
            OnRoadChanged?.Invoke(pos);
        }

        public void ClearRoadPreview()
        {
            var positionsToUpdate = new List<Vector2Int>(roadPreview);
            roadPreview.Clear();

            foreach (var position in positionsToUpdate)
            {
                OnRoadChanged?.Invoke(position);
            }
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

        public bool IsValidPlacement(Vector2Int tileToCheck, bool excludeRoadTiles = false)
        {
            foreach (var tile in occupiedTiles)
            {
                if (tile.Position != tileToCheck)
                    continue;

                if (excludeRoadTiles && tile.BuildingDefinition == BuildingDefinition.Road)
                    continue;

                return false;
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
