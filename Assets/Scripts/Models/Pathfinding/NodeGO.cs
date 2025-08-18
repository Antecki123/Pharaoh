using System.Collections.Generic;
using UnityEngine;

namespace Models.Ai.Pathfinding
{
    public class NodeGO : MonoBehaviour
    {
        [SerializeField] private List<NodeGO> neighbours = new List<NodeGO>();
        [HideInInspector] public Node<Vector2> Node;

        public List<NodeGO> GetNeighbours() => neighbours;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var n in neighbours)
            {
                if (n != null)
                    Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }
    }
}
