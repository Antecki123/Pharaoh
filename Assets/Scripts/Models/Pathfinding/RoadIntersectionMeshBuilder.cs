using Models.Ai;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Views.Road
{
    public struct Corner
    {
        public Vector3 point;
        public Vector3 dir;
        public int outletId;
        public bool isLeft;
    }

    public class RoadIntersectionGenerator
    {
        private readonly NavigationGraph navigationGraph;
        private readonly float roadWidth = .5f;

        public RoadIntersectionGenerator(NavigationGraph navigationGraph)
        {
            this.navigationGraph = navigationGraph;
        }

        public void GenerateIntersection(Vector3 position)
        {
            if (navigationGraph == null || navigationGraph.Nodes == null)
            {
                Debug.LogError("NavigationGraph is missing or has no nodes.");
                return;
            }

            var node = navigationGraph.GetNode(position);
            if (node != null && node.Neighbors.Count >= 3 /*&& !navigationGraph.Intersections.Contains(position)*/)
            {
                var mesh = GenerateMesh(node.Position, node.Neighbors.Select(n => n.Position), roadWidth);

                var go = new GameObject($"IntersectionMesh {node.NodeId}");
                go.transform.position = Vector3.zero + new Vector3(0, .01f, 0);

                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;

                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = Color.gray
                };

                navigationGraph.Intersections.Add(position);
            }
        }

        private Mesh GenerateMesh(Vector3 center, IEnumerable<Vector3> neighbors, float roadWidth)
        {
            var corners = new List<Corner>();
            var outletId = 0;
            var distanceFromCenter = navigationGraph.SegmentSpacing / 2f;

            foreach (var neighbor in neighbors)
            {
                var dir = (neighbor - center).normalized;
                var pointOnLine = center + dir * distanceFromCenter;

                var perp = Vector3.Cross(Vector3.up, dir).normalized;
                var leftPoint = pointOnLine - perp * (roadWidth * 0.5f);
                var rightPoint = pointOnLine + perp * (roadWidth * 0.5f);

                corners.Add(new Corner { point = leftPoint, outletId = outletId, isLeft = true, dir = dir });
                corners.Add(new Corner { point = rightPoint, outletId = outletId, isLeft = false, dir = dir });
                outletId++;
            }

            if (corners.Count < 3)
                return new Mesh();

            corners = corners
                .OrderBy(c => Mathf.Atan2(c.point.z - center.z, c.point.x - center.x))
                .ToList();

            var outline = new List<Vector3>();
            var straightEpsDeg = 5f;
            var bezierStrength = 0.6f;
            var bezierResolution = 6;

            var mesh = new Mesh();
            var triangles = new List<int>();

            for (int i = 0; i < corners.Count; i++)
            {
                var cur = corners[i];
                var nxt = corners[(i + 1) % corners.Count];

                outline.Add(cur.point);

                if (cur.outletId == nxt.outletId)
                {
                    continue;
                }
                var angle = Vector3.Angle(cur.dir, nxt.dir);
                var nearlyOpposite = angle >= (180f - straightEpsDeg);

                if (!nearlyOpposite)
                {
                    var arc = GenerateBezierCurve(cur.point, nxt.point, center, bezierStrength, bezierResolution);

                    if (arc.Count > 0) arc.RemoveAt(0);
                    outline.AddRange(arc);
                }
            }

            if (outline.Count < 3)
                return mesh;

            var meshVertices = new Vector3[outline.Count + 1];
            meshVertices[0] = center;

            for (int i = 0; i < outline.Count; i++)
                meshVertices[i + 1] = outline[i];

            mesh.SetVertices(meshVertices);

            for (int i = 1; i <= outline.Count; i++)
            {
                int next = (i % outline.Count) + 1;
                triangles.Add(0);
                triangles.Add(next);
                triangles.Add(i);
            }

            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        List<Vector3> GenerateBezierCurve(Vector3 a, Vector3 b, Vector3 center, float strength, int resolution)
        {
            var points = new List<Vector3>();
            var mid = (a + b) * 0.5f;
            var dir = (center - mid).normalized;
            var baseDist = Vector3.Distance(a, b) * 0.5f;
            var control = mid + baseDist * strength * dir;

            for (int i = 0; i <= resolution; i++)
            {
                var t = i / (float)resolution;
                var point = Mathf.Pow(1 - t, 2) * a + 2 * (1 - t) * t * control + Mathf.Pow(t, 2) * b;
                points.Add(point);
            }

            return points;
        }
    }
}