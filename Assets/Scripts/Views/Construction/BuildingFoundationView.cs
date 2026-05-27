using System.Linq;
using UnityEngine;

namespace Views.Construction
{
    public class BuildingFoundationView : MonoBehaviour
    {
        [SerializeField] private MeshFilter groundMesh;

        private readonly float zFightOffset = .05f;

        public void CalculateFoundationGround()
        {
            var groundTransform = groundMesh.transform;
            var vertices = groundMesh.mesh.vertices;

            var terrain = Terrain.activeTerrains.FirstOrDefault(t => t.gameObject.CompareTag("MainTerrain"));
            var terrainY = terrain.transform.position.y;

            for (int i = 0; i < vertices.Length; i++)
            {
                var worldPos = groundTransform.TransformPoint(vertices[i]);

                var height = terrain.SampleHeight(worldPos);
                worldPos.y = height + terrainY + zFightOffset;

                vertices[i] = groundTransform.InverseTransformPoint(worldPos);
            }

            groundMesh.mesh.vertices = vertices;
            groundMesh.mesh.RecalculateNormals();
            groundMesh.mesh.RecalculateBounds();
        }

        public void GenerateFoundationObjects()
        {

        }
    }
}