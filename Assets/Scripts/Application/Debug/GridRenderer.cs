using Models.Construction;
using Models.Environment;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace App.Debug
{
    public class GridRenderer : MonoBehaviour
    {
        private ConstructionGrid constructionGrid;
        private IrrigationModel irrigationModel;

        private readonly int size = 250;

        [Inject]
        public void Constructor(ConstructionGrid constructionGrid)
        {
            this.constructionGrid = constructionGrid;
        }

        private void Start()
        {
            CreateGridLineRenderer((x, z) => Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z)));
        }

        public void CreateGridLineRenderer(Func<float, float, float> terrainHeightFunc, Material sharedMaterial = null)
        {
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (sharedMaterial == null)
            {
                sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                sharedMaterial.color = Color.crimson;
            }

            meshRenderer.material = sharedMaterial;

            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            int cells = Mathf.FloorToInt(size);

            var vertices = new List<Vector3>(cells * cells * 4);
            var indices = new List<int>(cells * cells * 8);

            float offset = 0.5f;
            float heightOffset = 0.05f;

            for (int y = 0; y < cells; y++)
            {
                for (int x = 0; x < cells; x++)
                {
                    float centerX = x + offset;
                    float centerZ = y + offset;

                    Vector3 bl = new(
                        centerX - offset,
                        terrainHeightFunc(centerX - offset, centerZ - offset) + heightOffset,
                        centerZ - offset);

                    Vector3 tl = new(
                        centerX - offset,
                        terrainHeightFunc(centerX - offset, centerZ + offset) + heightOffset,
                        centerZ + offset);

                    Vector3 tr = new(
                        centerX + offset,
                        terrainHeightFunc(centerX + offset, centerZ + offset) + heightOffset,
                        centerZ + offset);

                    Vector3 br = new(
                        centerX + offset,
                        terrainHeightFunc(centerX + offset, centerZ - offset) + heightOffset,
                        centerZ - offset);

                    int baseIndex = vertices.Count;

                    vertices.Add(bl);
                    vertices.Add(tl);
                    vertices.Add(tr);
                    vertices.Add(br);

                    indices.Add(baseIndex + 0); indices.Add(baseIndex + 1);
                    indices.Add(baseIndex + 1); indices.Add(baseIndex + 2);
                    indices.Add(baseIndex + 2); indices.Add(baseIndex + 3);
                    indices.Add(baseIndex + 3); indices.Add(baseIndex + 0);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var tile in constructionGrid.RoadsTiles)
            {
                Gizmos.color = Color.darkGray;
                var x = tile.x + .5f;
                var z = tile.y + .5f;
                var h = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));

                Gizmos.DrawSphere(new Vector3(x, h, z), .15f);
            }

            foreach (var tile in constructionGrid.OccupiedTilesWithoutRoads)
            {
                Gizmos.color = Color.darkRed;
                var x = tile.x + .5f;
                var z = tile.y + .5f;
                var h = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));

                Gizmos.DrawSphere(new Vector3(x, h, z), .15f);
            }
        }
    }
}