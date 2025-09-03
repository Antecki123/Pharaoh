using System.Collections.Generic;
using UnityEngine;
using Views.Road;

namespace Models.Ai
{
    public class NavigationGraph
    {
        public HashSet<RoadNode> Nodes { get; } = new HashSet<RoadNode>();
        public HashSet<Vector3> Intersections { get; } = new HashSet<Vector3>();

        public float SegmentSpacing { get; } = 1f;

        public float MinimumSpacing { get; } = .33f;

        public float MinimumRoadAngle { get; } = 30f;

        public RoadNode GetNode(Vector3 position)
        {
            foreach (var node in Nodes)
            {
                if (node.Position.x == position.x && node.Position.z == position.z)
                    return node;
            }

            return null;
        }

        public bool Contains(Vector3 position)
        {
            foreach (var node in Nodes)
            {
                if (node.Position.x == position.x && node.Position.z == position.z)
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
                    neighborsPosition.Add(node.Position);
                }
            }

            return neighborsPosition;
        }
    }
}