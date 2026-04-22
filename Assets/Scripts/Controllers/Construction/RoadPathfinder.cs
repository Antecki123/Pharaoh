using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Construction
{
    public class RoadPathfinder
    {
        private readonly HashSet<Vector2Int> occupied;

        private static readonly Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public RoadPathfinder(HashSet<Vector2Int> occupied)
        {
            this.occupied = occupied;
        }

        public List<Vector2Int> FindRoadPath(Vector2Int startPosition, Vector2Int endPosition)
        {
            if (startPosition == endPosition)
                return new List<Vector2Int> { startPosition };

            var closedSet = new HashSet<Vector2Int>();
            var openSet = new HashSet<Vector2Int> { startPosition };

            var cameFrom = new Dictionary<Vector2Int, Vector2Int>(64);
            var gScore = new Dictionary<Vector2Int, float>(64);
            var fScore = new Dictionary<Vector2Int, float>(64);

            gScore[startPosition] = 0f;
            fScore[startPosition] = HeuristicCost(startPosition, endPosition);

            while (openSet.Count > 0)
            {
                Vector2Int current = default;
                float bestScore = float.MaxValue;

                foreach (var pos in openSet)
                {
                    float score;
                    if (!fScore.TryGetValue(pos, out score))
                        score = float.MaxValue;

                    if (score < bestScore)
                    {
                        bestScore = score;
                        current = pos;
                    }
                }

                if (current == endPosition)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);
                closedSet.Add(current);

                float currentG = gScore[current];

                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2Int neighbor = current + directions[i];

                    if (closedSet.Contains(neighbor))
                        continue;

                    float tentativeG = currentG + GetMovementCost(neighbor);

                    bool hasG = gScore.TryGetValue(neighbor, out float existingG);

                    if (!hasG || tentativeG < existingG)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + HeuristicCost(neighbor, endPosition);

                        if (!hasG)
                            openSet.Add(neighbor);
                    }
                }
            }

            return new List<Vector2Int>();
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int>(32)
            {
                current
            };

            while (cameFrom.TryGetValue(current, out current))
            {
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private float HeuristicCost(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private float GetMovementCost(Vector2Int to)
        {
            return occupied.Contains(to) ? 10f : 1f;
        }
    }
}