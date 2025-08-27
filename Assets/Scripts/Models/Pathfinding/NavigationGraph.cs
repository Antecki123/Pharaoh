using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Road;

namespace Models.Ai
{
    public class NavigationGraph
    {
        public HashSet<RoadNode> Nodes { get; }  = new HashSet<RoadNode>();

        public float SegmentSpacing { get; } = 1f;

        public float MinimumSpacing { get; } = .25f;

        public RoadNode GetNode(Vector3 position)
        {
            return Nodes.FirstOrDefault(node => node.Position.x == position.x && node.Position.z == position.z);
        }
    }
}