using Models.Ai.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Ai
{
    public class NavigationGraph
    {
        public IReadOnlyList<Node<Vector3>> Nodes => nodes;

        private List<Node<Vector3>> nodes = new List<Node<Vector3>>();

        public IReadOnlyDictionary<NodeType, float> MovementCost { get; } = new Dictionary<NodeType, float>()
        {
            { NodeType.Road, 1 },
            { NodeType.Terrain, 10 },
            { NodeType.ShallowWater, 25 },
            { NodeType.Block, float.MaxValue },
        };

        public void AddNode(Vector3 nodePosition, NodeType nodeType)
        {
            if (Contains(nodePosition))
                return;

            var newNode = new Node<Vector3>(
                nodePosition,
                nodeType,
                (a, b) =>
                {
                    float dist = Vector3.Distance(a.Data, b.Data);
                    float multiplier = MovementCost[nodeType];
                    return dist * multiplier;
                },
                (a, goal) => Vector3.Distance(a.Data, goal.Data)
            );

            nodes.Add(newNode);

            foreach (var node in nodes)
            {
                var connectionRange = 1.5f;
                float dist = Vector3.Distance(newNode.Data, node.Data);
                if (dist <= connectionRange && node != newNode)
                {
                    if (!newNode.Neighbors.Contains(node))
                        newNode.Neighbors.Add(node);

                    if (!node.Neighbors.Contains(newNode))
                        node.Neighbors.Add(newNode);
                }
            }
        }

        public void RemoveNode(Vector3 nodePosition, NodeType nodeType)
        {

        }

        public Node<Vector3> GetNode(Vector3 position)
        {
            foreach (var node in nodes)
            {
                if (node.Data.x == position.x && node.Data.z == position.z)
                    return node;
            }

            return null;
        }

        public Node<Vector3> GetClosestNode(Vector3 position, float range = 20f)
        {
            float rangeSqr = range * range;

            Node<Vector3> closestNode = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (var node in nodes)
            {
                float dx = node.Data.x - position.x;
                float dz = node.Data.z - position.z;
                float distanceSqr = dx * dx + dz * dz;

                if (distanceSqr <= rangeSqr && distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestNode = node;
                }
            }

            return closestNode;
        }

        public bool Contains(Vector3 position)
        {
            foreach (var node in nodes)
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