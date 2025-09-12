using Models.Ai.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Ai
{
    public class NavigationGraph
    {
        public HashSet<Node<Vector3>> Nodes { get; } = new HashSet<Node<Vector3>>();
        public HashSet<Vector3> Intersections { get; } = new HashSet<Vector3>();

        public Dictionary<NodeType, float> MovementCost { get; } = new Dictionary<NodeType, float>()
        {
            {NodeType.Road, 1 },
            {NodeType.Terrain, 10 },
            {NodeType.Block, float.MaxValue },
        };

        private Vector2 gridDimensions = new Vector2(50, 50);
        private float spacing = 2f;

        public NavigationGraph()
        {
            var offset = new Vector3((gridDimensions.x - 1) * spacing / 2f, 0, (gridDimensions.y - 1) * spacing / 2f);
            var nodeGrid = new Dictionary<(int x, int z), Node<Vector3>>();

            for (int x = 0; x < gridDimensions.x; x++)
            {
                for (int z = 0; z < gridDimensions.y; z++)
                {
                    var pos = new Vector3(x * spacing, 0, z * spacing) - offset;
                    var nodeType = NodeType.Terrain;

                    var node = new Node<Vector3>(
                        pos, nodeType,
                        (a, b) =>
                        {
                            float dist = Vector3.Distance(a.Data, b.Data);
                            float multiplier = MovementCost[nodeType];
                            return dist * multiplier;
                        },
                        (a, goal) => Vector3.Distance(a.Data, goal.Data)
                    );

                    Nodes.Add(node);
                    nodeGrid[(x, z)] = node;
                }
            }

            for (int x = 0; x < gridDimensions.x; x++)
            {
                for (int z = 0; z < gridDimensions.y; z++)
                {
                    var node = nodeGrid[(x, z)];

                    if (nodeGrid.TryGetValue((x + 1, z), out var right)) node.Neighbors.Add(right);
                    if (nodeGrid.TryGetValue((x - 1, z), out var left)) node.Neighbors.Add(left);
                    if (nodeGrid.TryGetValue((x, z + 1), out var up)) node.Neighbors.Add(up);
                    if (nodeGrid.TryGetValue((x, z - 1), out var down)) node.Neighbors.Add(down);

                    if (nodeGrid.TryGetValue((x + 1, z + 1), out var ur)) node.Neighbors.Add(ur);
                    if (nodeGrid.TryGetValue((x + 1, z - 1), out var dr)) node.Neighbors.Add(dr);
                    if (nodeGrid.TryGetValue((x - 1, z + 1), out var ul)) node.Neighbors.Add(ul);
                    if (nodeGrid.TryGetValue((x - 1, z - 1), out var dl)) node.Neighbors.Add(dl);
                }
            }
        }

        public Node<Vector3> GetNode(Vector3 position)
        {
            foreach (var node in Nodes)
            {
                if (node.Data.x == position.x && node.Data.z == position.z)
                    return node;
            }

            return null;
        }

        public bool Contains(Vector3 position)
        {
            foreach (var node in Nodes)
            {
                if (node.Data.x == position.x && node.Data.z == position.z)
                    return true;
            }

            return false;
        }

        public List<Vector3> GetNeighborsPosition(Vector3 position)
        {
            var neighborsPosition = new List<Vector3>();

            if (Contains(position))
            {
                foreach (var node in GetNode(position).Neighbors)
                {
                    if (node.NodeType == NodeType.Road)
                        neighborsPosition.Add(node.Data);
                }
            }

            return neighborsPosition;
        }
    }
}