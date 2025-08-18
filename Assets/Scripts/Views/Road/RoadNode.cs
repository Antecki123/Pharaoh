using System.Collections.Generic;
using UnityEngine;

namespace Views.Road
{
    public class RoadNode : MonoBehaviour
    {
        public Vector3 Position { get; set; }
        public List<RoadNode> Neighbors { get; set; } = new List<RoadNode>();
    }
}