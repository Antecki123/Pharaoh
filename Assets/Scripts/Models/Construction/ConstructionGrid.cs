using System;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;

namespace Models.Construction
{
    public enum TileType
    {
        Water,
        Cliff,
        Road,
        Building,
        Blocked
    }

    public class ConstructionGrid
    {
        public event Action OnValueChanged;
        public event Action<Vector2Int> OnRoadChanged;

        public HashSet<Vector2Int> OccupiedTilesWithoutRoads => occupiedTilesWithoutRoads;
        public HashSet<Vector2Int> RoadsTiles => roadsTiles;
        public HashSet<Vector2Int> RoadPreview => roadPreview;

        private readonly Dictionary<Vector2Int, ConstructionGridData> tilesByPosition = new Dictionary<Vector2Int, ConstructionGridData>();
        private readonly HashSet<Vector2Int> occupiedTilesWithoutRoads = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> roadsTiles = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> roadPreview = new HashSet<Vector2Int>();

        private readonly List<Vector2Int> removeBuffer = new List<Vector2Int>(32);

        private static readonly Vector2Int[] Neighbours = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public void AddOccupant(Vector2Int pos, TileType tileType, BuildingView buildingView = null)
        {
            if (!tilesByPosition.ContainsKey(pos))
            {
                tilesByPosition[pos] = new ConstructionGridData
                {
                    Position = pos,
                    TileType = tileType,
                    BuildingView = buildingView
                };
            }

            if (tileType != TileType.Road)
                occupiedTilesWithoutRoads.Add(pos);
            else
                roadsTiles.Add(pos);

            OnValueChanged?.Invoke();

            if (tileType == TileType.Road)
                OnRoadChanged?.Invoke(pos);
        }

        public void AddOccupant(List<Vector2Int> cells, TileType tileType, BuildingView buildingView)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var pos = cells[i];
                if (!tilesByPosition.ContainsKey(pos))
                {
                    tilesByPosition[pos] = new ConstructionGridData
                    {
                        Position = pos,
                        TileType = tileType,
                        BuildingView = buildingView
                    };
                }

                if (tileType != TileType.Road)
                    occupiedTilesWithoutRoads.Add(pos);
                else
                    roadsTiles.Add(pos);
            }

            OnValueChanged?.Invoke();
        }

        public void RemoveOccupant(Vector2Int cellToRemove)
        {
            if (!tilesByPosition.TryGetValue(cellToRemove, out var tile))
                return;

            if (tile.BuildingView == null)
                return;

            var building = tile.BuildingView;
            var tileType = tile.TileType;

            removeBuffer.Clear();
            foreach (var kvp in tilesByPosition)
            {
                if (kvp.Value.BuildingView == building)
                    removeBuffer.Add(kvp.Key);
            }

            for (int i = 0; i < removeBuffer.Count; i++)
            {
                var pos = removeBuffer[i];
                tilesByPosition.Remove(pos);
                occupiedTilesWithoutRoads.Remove(pos);
                roadsTiles.Remove(pos);
            }

            OnValueChanged?.Invoke();

            if (tileType == TileType.Road)
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
            removeBuffer.Clear();
            removeBuffer.AddRange(roadPreview);
            roadPreview.Clear();

            for (int i = 0; i < removeBuffer.Count; i++)
                OnRoadChanged?.Invoke(removeBuffer[i]);
        }

        public bool IsValidPlacement(List<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (tilesByPosition.ContainsKey(cells[i]))
                    return false;
            }
            return true;
        }

        public bool IsValidPlacement(Vector2Int tileToCheck, bool excludeRoadTiles = false)
        {
            if (!tilesByPosition.TryGetValue(tileToCheck, out var tile))
                return true;

            if (excludeRoadTiles && tile.TileType == TileType.Road)
                return true;

            return false;
        }

        public bool HasRoadConnection(BuildingView buildingView)
        {
            foreach (var kvp in tilesByPosition)
            {
                if (kvp.Value.BuildingView != buildingView)
                    continue;

                var pos = kvp.Key;
                for (int i = 0; i < Neighbours.Length; i++)
                {
                    if (tilesByPosition.TryGetValue(pos + Neighbours[i], out var neighbour)
                        && neighbour.TileType == TileType.Road)
                        return true;
                }
            }
            return false;
        }

        public List<Vector2Int> GetAllConnectedRoadTiles(BuildingView buildingView)
        {
            var roadTiles = new List<Vector2Int>();

            foreach (var kvp in tilesByPosition)
            {
                if (kvp.Value.BuildingView != buildingView)
                    continue;

                var pos = kvp.Key;
                for (int i = 0; i < Neighbours.Length; i++)
                {
                    var neighbourPos = pos + Neighbours[i];
                    if (tilesByPosition.TryGetValue(neighbourPos, out var neighbour)
                        && neighbour.TileType == TileType.Road
                        && !roadTiles.Contains(neighbourPos))
                    {
                        roadTiles.Add(neighbourPos);
                    }
                }
            }

            return roadTiles;
        }

        public ConstructionGridData GetTileByPosition(Vector2Int position)
        {
            tilesByPosition.TryGetValue(position, out var tile);
            return tile;
        }
    }

    public class ConstructionGridData
    {
        public Vector2Int Position { get; set; }
        public TileType TileType { get; set; }
        public BuildingView BuildingView { get; set; }

        public override bool Equals(object obj)
            => obj is ConstructionGridData other && Position.Equals(other.Position);

        public override int GetHashCode()
            => Position.GetHashCode();
    }

    [Serializable]
    public class TileData
    {
        public Vector2Int cell;
        public TileType type;

        public TileData(Vector2Int c, TileType t)
        {
            cell = c;
            type = t;
        }
    }

    [Serializable]
    public class TileDataCollection
    {
        public List<TileData> tiles = new List<TileData>();
    }
}