using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Construction
{
    public class RoadPathfinder
    {
        private readonly HashSet<Vector2Int> occupied;
        private static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        private readonly Dictionary<Vector2Int, float> gScore = new(256);
        private readonly Dictionary<Vector2Int, Vector2Int> cameFrom = new(256);
        private readonly HashSet<Vector2Int> closedSet = new(256);
        private readonly MinHeap<Vector2Int> openQueue = new();
        private readonly HashSet<Vector2Int> openSet = new(256);

        public RoadPathfinder(HashSet<Vector2Int> occupied)
        {
            this.occupied = occupied;
        }

        public void FindRoadPath(Vector2Int start, Vector2Int end, List<Vector2Int> route, float maxDistance = float.MaxValue)
        {
            if (start == end)
            {
                route.Clear();
                route.Add(start);
                return;
            }

            gScore.Clear();
            cameFrom.Clear();
            closedSet.Clear();
            openSet.Clear();

            while (openQueue.Count > 0) 
                openQueue.Dequeue();

            gScore[start] = 0f;
            openQueue.Enqueue(start, Heuristic(start, end));
            openSet.Add(start);

            while (openQueue.Count > 0)
            {
                var current = openQueue.Dequeue();
                openSet.Remove(current);

                if (current == end)
                {
                    ReconstructPath(current, route);
                    return;
                }

                closedSet.Add(current);
                float currentG = gScore[current];

                for (int i = 0; i < directions.Length; i++)
                {
                    var neighbor = current + directions[i];

                    if (closedSet.Contains(neighbor))
                        continue;

                    if (Heuristic(neighbor, end) > maxDistance)
                        continue;

                    float tentativeG = currentG + GetMovementCost(neighbor);

                    if (gScore.TryGetValue(neighbor, out float existingG)
                        && tentativeG >= existingG)
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    float f = tentativeG + Heuristic(neighbor, end);

                    if (!openSet.Contains(neighbor))
                    {
                        openQueue.Enqueue(neighbor, f);
                        openSet.Add(neighbor);
                    }
                    else
                    {
                        openQueue.Enqueue(neighbor, f);
                    }
                }
            }

            return;
        }

        private void ReconstructPath(Vector2Int current, List<Vector2Int> route)
        {
            route.Clear();
            route.Add(current);

            while (cameFrom.TryGetValue(current, out current))
                route.Add(current);

            route.Reverse();
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        private float GetMovementCost(Vector2Int to)
            => occupied.Contains(to) ? 10f : 1f;
    }

    public class MinHeap<T>
    {
        private readonly List<(T item, float priority)> _heap = new(64);

        public int Count => _heap.Count;

        public void Enqueue(T item, float priority)
        {
            _heap.Add((item, priority));
            BubbleUp(_heap.Count - 1);
        }

        public T Dequeue()
        {
            T top = _heap[0].item;
            int last = _heap.Count - 1;
            _heap[0] = _heap[last];
            _heap.RemoveAt(last);
            if (_heap.Count > 0) SiftDown(0);
            return top;
        }

        public void Clear() => _heap.Clear();

        private void BubbleUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_heap[parent].priority <= _heap[i].priority) break;
                (_heap[i], _heap[parent]) = (_heap[parent], _heap[i]);
                i = parent;
            }
        }

        private void SiftDown(int i)
        {
            int count = _heap.Count;
            while (true)
            {
                int smallest = i;
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                if (left < count && _heap[left].priority < _heap[smallest].priority) smallest = left;
                if (right < count && _heap[right].priority < _heap[smallest].priority) smallest = right;
                if (smallest == i) break;
                (_heap[i], _heap[smallest]) = (_heap[smallest], _heap[i]);
                i = smallest;
            }
        }
    }
}