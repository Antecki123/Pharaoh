using UnityEngine;

namespace Controllers.Construction
{
    public class SelectionMaskView : MonoBehaviour
    {
        private const int samplesPerUnit = 4;
        private const float zFightOffset = 0.1f;

        private MeshFilter meshFilter;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        public void UpdateMask(Vector2Int startPoint, Vector2Int endPoint)
        {
            var terrain = Terrain.activeTerrain;
            float terrainY = terrain != null ? terrain.transform.position.y : 0f;

            int minX = Mathf.Min(startPoint.x, endPoint.x);
            int minZ = Mathf.Min(startPoint.y, endPoint.y);

            int sizeX = Mathf.Max(1, Mathf.Abs(endPoint.x - startPoint.x));
            int sizeZ = Mathf.Max(1, Mathf.Abs(endPoint.y - startPoint.y));

            int segmentsX = sizeX * samplesPerUnit;
            int segmentsZ = sizeZ * samplesPerUnit;

            int vertsX = segmentsX + 1;
            int vertsZ = segmentsZ + 1;

            var vertices = new Vector3[vertsX * vertsZ];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[segmentsX * segmentsZ * 6];

            int v = 0;
            for (int z = 0; z <= segmentsZ; z++)
            {
                for (int x = 0; x <= segmentsX; x++)
                {
                    float localX = minX + (float)x / samplesPerUnit;
                    float localZ = minZ + (float)z / samplesPerUnit;

                    var worldPos = transform.TransformPoint(new Vector3(localX, 0f, localZ));
                    float height = 0f;
                    if (terrain != null)
                    {
                        height = terrain.SampleHeight(worldPos)
                                 + terrainY
                                 + zFightOffset;
                    }
                    worldPos.y = height;
                    vertices[v] = transform.InverseTransformPoint(worldPos);
                    uvs[v] = new Vector2((float)x / segmentsX, (float)z / segmentsZ);
                    v++;
                }
            }

            int t = 0;
            for (int z = 0; z < segmentsZ; z++)
            {
                for (int x = 0; x < segmentsX; x++)
                {
                    int i = z * vertsX + x;
                    triangles[t++] = i;
                    triangles[t++] = i + vertsX;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + vertsX;
                    triangles[t++] = i + vertsX + 1;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.mesh = mesh;
        }
    }
}