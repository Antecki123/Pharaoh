using System;
using System.Collections.Generic;
using UnityEngine;

namespace Views.Road
{
    public class RoadNode : IEquatable<RoadNode>
    {
        public Guid NodeId { get; private set; }
        public Vector3 Position { get; private set; }
        public List<RoadNode> Neighbors { get; set; } = new List<RoadNode>();

        public RoadNode(Vector3 position)
        {
            NodeId = Guid.NewGuid();
            Position = position;
        }

        public bool Equals(RoadNode other)
        {
            if (other == null) return false;

            return Position.x == other.Position.x &&
                   Position.z == other.Position.z;
        }

        public override bool Equals(object obj) => Equals(obj as RoadNode);

        public override int GetHashCode() => HashCode.Combine(Position.x, Position.z);
    }
}