using System.Collections.Generic;
using UnityEngine;

namespace Views.Helpers
{
    public class MeshBuilder
    {
        public static float CalculatePolygonArea(List<Vector3> farmVertices)
        {
            if (farmVertices == null || farmVertices.Count < 3)
                return 0f;

            float area = 0f;

            for (int i = 0; i < farmVertices.Count; i++)
            {
                Vector3 p1 = farmVertices[i];
                Vector3 p2 = farmVertices[(i + 1) % farmVertices.Count];

                area += (p1.x * p2.z) - (p2.x * p1.z);
            }

            return Mathf.Abs(area) * 0.5f;
        }

        public static Vector3 GetPolygonCentroid(List<Vector3> vertices)
        {
            if (vertices == null || vertices.Count < 3)
                throw new System.ArgumentException("At least 3 vertices required to compute centroid.");

            float signedArea = 0f;
            float cx = 0f;
            float cz = 0f;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];

                float a = current.x * next.z - next.x * current.z;
                signedArea += a;
                cx += (current.x + next.x) * a;
                cz += (current.z + next.z) * a;
            }

            signedArea *= 0.5f;

            if (Mathf.Abs(signedArea) < Mathf.Epsilon)
                return vertices[0];

            cx /= (6f * signedArea);
            cz /= (6f * signedArea);

            float avgY = 0f;
            foreach (var v in vertices)
                avgY += v.y;
            avgY /= vertices.Count;

            return new Vector3(cx, avgY, cz);
        }

        public static Mesh BuildMeshFromVertices(List<Vector3> farmVertices, out Vector3 centroid)
        {
            if (farmVertices == null || farmVertices.Count < 3)
            {
                Debug.LogWarning("Not enough vertices to build a mesh.");
                centroid = Vector3.zero;
                return null;
            }

            centroid = GetPolygonCentroid(farmVertices);

            Vector3[] vertices = new Vector3[farmVertices.Count];
            for (int i = 0; i < farmVertices.Count; i++)
            {
                Vector3 v = farmVertices[i];
                vertices[i] = new Vector3(v.x - centroid.x, v.y - centroid.y + 0.1f, v.z - centroid.z);
            }

            Vector2[] points2D = new Vector2[farmVertices.Count];
            for (int i = 0; i < farmVertices.Count; i++)
                points2D[i] = new Vector2(farmVertices[i].x, farmVertices[i].z);

            int[] indices = Triangulate(points2D);

            Vector2[] uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                uv[i] = new Vector2(vertices[i].x, vertices[i].z);

            Mesh mesh = new Mesh()
            {
                name = "FarmMesh",
                vertices = vertices,
                triangles = indices,
                uv = uv
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public static Mesh BuildColliderFromVertices(List<Vector3> vertices)
        {
            if (vertices == null || vertices.Count < 3)
            {
                Debug.LogWarning("Not enough vertices to build a collider.");
                return null;
            }

            var centroid = GetPolygonCentroid(vertices);

            float height = 1f;
            float halfHeight = height / 2f;

            Vector3[] topVertices = new Vector3[vertices.Count];
            Vector3[] bottomVertices = new Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                Vector3 local = v - centroid;
                topVertices[i] = new Vector3(local.x, local.y + halfHeight, local.z);
                bottomVertices[i] = new Vector3(local.x, local.y - halfHeight, local.z);
            }

            List<Vector3> allVertices = new List<Vector3>();
            allVertices.AddRange(topVertices);
            allVertices.AddRange(bottomVertices);

            Vector2[] points2D = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                points2D[i] = new Vector2(vertices[i].x, vertices[i].z);

            int[] topTriangles = Triangulate(points2D);
            int[] bottomTriangles = Triangulate(points2D);

            for (int i = 0; i < bottomTriangles.Length; i += 3)
            {
                int temp = bottomTriangles[i];
                bottomTriangles[i] = bottomTriangles[i + 1];
                bottomTriangles[i + 1] = temp;

                bottomTriangles[i] += vertices.Count;
                bottomTriangles[i + 1] += vertices.Count;
                bottomTriangles[i + 2] += vertices.Count;
            }

            List<int> sideTriangles = new List<int>();
            for (int i = 0; i < vertices.Count; i++)
            {
                int next = (i + 1) % vertices.Count;

                int topA = i;
                int topB = next;
                int bottomA = i + vertices.Count;
                int bottomB = next + vertices.Count;

                sideTriangles.Add(topA);
                sideTriangles.Add(bottomA);
                sideTriangles.Add(bottomB);

                sideTriangles.Add(topA);
                sideTriangles.Add(bottomB);
                sideTriangles.Add(topB);
            }

            List<int> allTriangles = new List<int>();
            allTriangles.AddRange(topTriangles);
            allTriangles.AddRange(bottomTriangles);
            allTriangles.AddRange(sideTriangles);

            Mesh colliderMesh = new Mesh();
            colliderMesh.name = "FarmColliderMesh";
            colliderMesh.vertices = allVertices.ToArray();
            colliderMesh.triangles = allTriangles.ToArray();
            colliderMesh.RecalculateNormals();
            colliderMesh.RecalculateBounds();

            return colliderMesh;
        }

        private static int[] Triangulate(Vector2[] points)
        {
            var indices = new List<int>();
            int n = points.Length;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (GetArea(points) > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if (count-- <= 0)
                    break;

                int u = v; if (nv <= u) u = 0;
                v = u + 1; if (nv <= v) v = 0;
                int w = v + 1; if (nv <= w) w = 0;

                if (Snip(points, u, v, w, nv, V))
                {
                    int a = V[u], b = V[v], c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (int s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private static float GetArea(Vector2[] points)
        {
            int n = points.Length;
            float A = 0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
                A += points[p].x * points[q].y - points[q].x * points[p].y;
            return A * 0.5f;
        }

        private static bool Snip(Vector2[] points, int u, int v, int w, int n, int[] V)
        {
            Vector2 A = points[V[u]];
            Vector2 B = points[V[v]];
            Vector2 C = points[V[w]];

            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;

            for (int p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax = C.x - B.x, ay = C.y - B.y;
            float bx = A.x - C.x, by = A.y - C.y;
            float cx = B.x - A.x, cy = B.y - A.y;
            float apx = P.x - A.x, apy = P.y - A.y;
            float bpx = P.x - B.x, bpy = P.y - B.y;
            float cpx = P.x - C.x, cpy = P.y - C.y;

            float aCROSSbp = ax * bpy - ay * bpx;
            float cCROSSap = cx * apy - cy * apx;
            float bCROSScp = bx * cpy - by * cpx;

            return (aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f);
        }
    }
}