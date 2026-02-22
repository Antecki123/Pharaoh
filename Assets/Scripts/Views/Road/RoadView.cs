using App.Configs;
using Models.Ai;
using Models.Ai.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Views.Road
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadView : BuildingView
    {
        private NavigationGraph navigationGraph;
        private ConstructionConfig constructionConfig;

        private readonly float zFightOffset = .05f;

        [Inject]
        public void Constructor(NavigationGraph navigationGraph, ConstructionConfig constructionConfig)
        {
            this.navigationGraph = navigationGraph;
            this.constructionConfig = constructionConfig;
        }

        public void Init(Vector3 startPos, Vector3 endPos)
        {
            GenerateMesh(startPos, endPos);

            var roadIntersectionGenerator = new RoadIntersectionGenerator(navigationGraph, constructionConfig);
            roadIntersectionGenerator.GenerateIntersection(startPos);
            roadIntersectionGenerator.GenerateIntersection(endPos);
        }

        public void Init(Vector3 centerPosition)
        {
            GenerateMesh(centerPosition);
            GenerateNavigationNodes(centerPosition);
        }

        private void GenerateMesh(Vector3 startPos, Vector3 endPos)
        {
            var mesh = new Mesh();
            var direction = (endPos - startPos).normalized;
            var flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            var right = Vector3.Cross(Vector3.up, flatDirection).normalized * (constructionConfig.RoadWidth / 2f);

            var v0 = startPos - right;
            var v1 = startPos + right;
            var v2 = endPos - right;
            var v3 = endPos + right;

            v0 = transform.InverseTransformPoint(v0);
            v1 = transform.InverseTransformPoint(v1);
            v2 = transform.InverseTransformPoint(v2);
            v3 = transform.InverseTransformPoint(v3);

            mesh.vertices = new Vector3[] { v0, v1, v2, v3 };

            mesh.triangles = new int[]
            {
                0, 2, 1,
                2, 3, 1
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = Color.gray
            };
        }

        private void GenerateMesh(Vector3 centerPosition)
        {
            var size = 4f;
            var resolution = 10;
            var half = size * 0.5f;
            var step = size / resolution;

            var terrain = Terrain.activeTerrain;
            var terrainY = terrain != null ? terrain.transform.position.y : 0f;

            var vertPerLine = resolution + 1;
            var vertices = new Vector3[vertPerLine * vertPerLine];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[resolution * resolution * 6];

            var v = 0;
            for (var z = 0; z <= resolution; z++)
            {
                for (var x = 0; x <= resolution; x++)
                {
                    var worldX = centerPosition.x - half + x * step;
                    var worldZ = centerPosition.z - half + z * step;

                    var height = 0f;
                    if (terrain != null)
                    {
                        height = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrainY + zFightOffset;
                    }

                    vertices[v] = new Vector3(worldX, height, worldZ);
                    uvs[v] = new Vector2((float)x / resolution, (float)z / resolution);
                    v++;
                }
            }

            var t = 0;
            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var i = z * vertPerLine + x;

                    triangles[t++] = i;
                    triangles[t++] = i + vertPerLine;
                    triangles[t++] = i + 1;

                    triangles[t++] = i + 1;
                    triangles[t++] = i + vertPerLine;
                    triangles[t++] = i + vertPerLine + 1;
                }
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var filter = GetComponent<MeshFilter>();
            filter.mesh = mesh;

            var renderer = GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = Color.gray
            };
        }

        private void GenerateNavigationNodes(Vector3 center)
        {
            var cellSize = 2f;
            var resolution = 2;
            var nodesPositions = new List<Vector3>();
            var offset = (resolution - 1) * cellSize / 2f;

            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    var xPos = center.x - offset + (x * cellSize);
                    var zPos = center.z - offset + (z * cellSize);
                    var height = Terrain.activeTerrain.SampleHeight(new Vector3(xPos, 0, zPos)) + zFightOffset;
                    nodesPositions.Add(new Vector3(xPos, height, zPos));
                }
            }

            foreach (var position in nodesPositions)
            {
                navigationGraph.AddNode(position, NodeType.Road);
                this.navigationNodes.Add(position);
            }

            return;

            var navigationNodes = new List<Node<Vector3>>();
            for (int i = 0; i < nodesPositions.Count; i++)
            {
                var nodePosition = nodesPositions[i];
                Node<Vector3> node;

                node = navigationGraph.GetNode(nodePosition);
                if (node == null)
                {
                    var nodeType = NodeType.Road;
                    node = new Node<Vector3>(
                        nodePosition,
                        nodeType,
                        (a, b) =>
                        {
                            float dist = Vector3.Distance(a.Data, b.Data);
                            float multiplier = navigationGraph.MovementCost[nodeType];
                            return dist * multiplier;
                        },
                        (a, goal) => Vector3.Distance(a.Data, goal.Data)
                    );

                    //navigationGraph.Nodes.Add(node);
                }

                navigationNodes.Add(node);
            }

            for (int i = 0; i < navigationNodes.Count - 1; i++)
            {
                var current = navigationNodes[i];
                var next = navigationNodes[i + 1];

                if (!current.Neighbors.Contains(next))
                    current.Neighbors.Add(next);

                if (!next.Neighbors.Contains(current))
                    next.Neighbors.Add(current);
            }

            var connectionRange = .5f;
            foreach (var roadNode in navigationNodes)
            {
                var nearbyTerrainNodes = navigationGraph.Nodes
                    .Where(n => n.NodeType == NodeType.Terrain)
                    .Where(n => Vector3.Distance(n.Data, roadNode.Data) <= connectionRange);

                foreach (var terrainNode in nearbyTerrainNodes)
                {
                    if (!roadNode.Neighbors.Contains(terrainNode))
                        roadNode.Neighbors.Add(terrainNode);

                    if (!terrainNode.Neighbors.Contains(roadNode))
                        terrainNode.Neighbors.Add(roadNode);
                }
            }
        }

        List<Vector3> navigationNodes = new List<Vector3>();

        private void OnDrawGizmosSelected()
        {
            foreach (var nodePosition in navigationNodes)
            {
                var node = navigationGraph.GetNode(nodePosition);
                Gizmos.DrawWireSphere(node.Data, .2f);

                foreach (var neighbor in node.Neighbors)
                {
                    Gizmos.DrawLine(nodePosition, neighbor.Data);
                }
            }
        }
    }
}