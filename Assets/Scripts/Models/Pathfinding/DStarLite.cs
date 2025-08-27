using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models.Ai.Pathfinding
{
    public interface IPathfindingBrain<T>
    {
        public void Initialize(List<Node<T>> allNodes, Node<T> startNode, Node<T> goalNode);

        public List<Node<T>> GetPath();
    }

    public class DStarLite<T> : IPathfindingBrain<T>
    {
        public class KeyNodeComparer : IComparer<(Key, Node<T>)>
        {
            public int Compare((Key, Node<T>) x, (Key, Node<T>) y)
            {
                return x.Item1 < y.Item1 ? -1 : x.Item1 > y.Item1 ? 1 : 0;
            }
        }

        private Node<T> startNode;
        private Node<T> goalNode;
        private float keyModifier;

        private readonly SortedSet<(Key, Node<T>)> openSet = new SortedSet<(Key, Node<T>)>(new KeyNodeComparer());
        private readonly Dictionary<Node<T>, Key> lookups = new Dictionary<Node<T>, Key>();

        private const int MAX_CYCLES = 1000;

        public void Initialize(List<Node<T>> allNodes, Node<T> startNode, Node<T> goalNode)
        {
            this.startNode = startNode;
            this.goalNode = goalNode;

            openSet.Clear();
            lookups.Clear();
            keyModifier = 0;

            foreach (var node in allNodes)
            {
                node.G = float.MaxValue;
                node.RHS = float.MaxValue;
            }

            goalNode.RHS = 0;
            var key = CalculateKey(goalNode);
            openSet.Add((key, goalNode));
            lookups[goalNode] = key;
        }

        public List<Node<T>> GetPath()
        {
            ComputeShortestPath();

            var path = new List<Node<T>> { startNode };
            var current = startNode;

            while (current != goalNode)
            {
                var next = current.Neighbors
                    .OrderBy(n => n.Cost(current, n) + n.G)
                    .FirstOrDefault();

                if (next == null) break;

                path.Add(next);
                current = next;
            }

            return path;
        }

        public void RecalculateNode(Node<T> node)
        {
            keyModifier += startNode.Heuristic(startNode, node);

            var allConnectedNodes = Successors(node).Concat(Predecessors(node)).ToList();

            foreach (var s in allConnectedNodes)
            {
                if (s != startNode)
                {
                    s.RHS = Mathf.Min(s.RHS, s.Cost(s, node) + node.G);
                }

                UpdateVertex(s);
            }

            UpdateVertex(node);
            ComputeShortestPath();
        }

        private void ComputeShortestPath()
        {
            var maxSteps = MAX_CYCLES;
            while (openSet.Count > 0 && (openSet.Min.Item1 < CalculateKey(startNode) || startNode.RHS > startNode.G))
            {
                if (maxSteps-- <= 0)
                {
                    Debug.LogWarning("ComputeShortestPath error: max steps exceeded.");
                    break;
                }

                var smallest = openSet.Min;
                openSet.Remove(smallest);
                lookups.Remove(smallest.Item2);
                var node = smallest.Item2;

                if (smallest.Item1 < CalculateKey(node))
                {
                    var newKey = CalculateKey(node);
                    openSet.Add((newKey, node));
                    lookups[node] = newKey;
                }
                else if (node.G > node.RHS)
                {
                    node.G = node.RHS;
                    foreach (var s in Predecessors(node))
                    {
                        if (s != goalNode)
                        {
                            s.RHS = Mathf.Min(s.RHS, s.Cost(s, node) + node.G);
                        }

                        UpdateVertex(s);
                    }
                }
                else
                {
                    var gOld = node.G;
                    node.G = float.MaxValue;
                    foreach (var s in Predecessors(node).Concat(new[] { node }))
                    {
                        if (s.RHS.Approx(s.Cost(s, node) + gOld))
                        {
                            if (s != goalNode)
                            {
                                s.RHS = float.MaxValue;
                            }

                            foreach (var sPrime in Successors(s))
                            {
                                s.RHS = Mathf.Min(s.RHS, s.Cost(s, sPrime) + sPrime.G);
                            }
                        }

                        UpdateVertex(s);
                    }
                }
            }

            startNode.G = startNode.RHS;
        }

        private IEnumerable<Node<T>> Predecessors(Node<T> node) => node.Neighbors;

        private IEnumerable<Node<T>> Successors(Node<T> node) => node.Neighbors;

        private void UpdateVertex(Node<T> node)
        {
            var key = CalculateKey(node);
            if (!node.GEqualRHS && !lookups.ContainsKey(node))
            {
                openSet.Add((key, node));
                lookups[node] = key;
            }
            else if (node.GEqualRHS && lookups.ContainsKey(node))
            {
                openSet.Remove((lookups[node], node));
                lookups.Remove(node);
            }
            else if (lookups.ContainsKey(node))
            {
                openSet.Remove((lookups[node], node));
                openSet.Add((key, node));
                lookups[node] = key;
            }
        }

        private Key CalculateKey(Node<T> node)
        {
            return new Key(
                Mathf.Min(node.G, node.RHS) + node.Heuristic(node, startNode) + keyModifier,
                Mathf.Min(node.G, node.RHS));
        }
    }
}