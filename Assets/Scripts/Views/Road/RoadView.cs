using Models.Ai;
using Models.Ai.Pathfinding;
using Models.Construction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Views.Construction;
using Zenject;

namespace Views.Road
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadView : BuildingView
    {
        [SerializeField] private SpriteAtlas roadTextures;
        private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        private MeshRenderer meshRenderer;
        private NavigationGraph navigationGraph;
        private ConstructionGrid constructionGrid;

        private readonly float zFightOffset = .03f;
        private Vector2Int position;

        private List<Vector3> navigationNodesForGizmos = new List<Vector3>();

        [Inject]
        public void Constructor(NavigationGraph navigationGraph, ConstructionGrid constructionGrid)
        {
            this.navigationGraph = navigationGraph;
            this.constructionGrid = constructionGrid;

            meshRenderer = GetComponent<MeshRenderer>();
            constructionGrid.OnRoadChanged += UpdateTileVisual;
        }

        private void OnDestroy()
        {
            constructionGrid.OnRoadChanged -= UpdateTileVisual;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            foreach (var material in meshRenderer.materials)
            {
                material.color = Color.white;
                GenerateNavigationNodes();
            }
        }

        public override void DestroyBuilding()
        {
            base.DestroyBuilding();
        }

        public void CreatePreview(Vector2Int position)
        {
            this.position = position;

            GenerateMesh();
            SetTexture();
        }

        public void SetColor(Color color)
        {
            foreach (var material in meshRenderer.materials)
            {
                material.color = color;
            }
        }

        private void GenerateMesh()
        {
            const int resolution = 10;

            var terrain = Terrain.activeTerrain;
            var terrainY = terrain != null ? terrain.transform.position.y : 0f;

            var vertPerLine = resolution + 1;

            var vertices = new Vector3[vertPerLine * vertPerLine];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[resolution * resolution * 6];

            var v = 0;

            for (int z = 0; z <= resolution; z++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    var localX = -0.5f + (float)x / resolution;
                    var localZ = -0.5f + (float)z / resolution;

                    var worldPos = transform.TransformPoint(new Vector3(localX, 0f, localZ));

                    var height = 0f;

                    if (terrain != null)
                    {
                        height = terrain.SampleHeight(worldPos)
                                 + terrainY
                                 + zFightOffset;
                    }

                    worldPos.y = height;
                    vertices[v] = transform.InverseTransformPoint(worldPos);
                    uvs[v] = new Vector2((float)x / resolution, (float)z / resolution);
                    v++;
                }
            }

            var t = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = z * vertPerLine + x;

                    triangles[t++] = i;
                    triangles[t++] = i + vertPerLine;
                    triangles[t++] = i + 1;

                    triangles[t++] = i + 1;
                    triangles[t++] = i + vertPerLine;
                    triangles[t++] = i + vertPerLine + 1;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private void UpdateTileVisual(Vector2Int changedTilePosition)
        {
            if (changedTilePosition != position
                && changedTilePosition != position + Vector2Int.up
                && changedTilePosition != position + Vector2Int.down
                && changedTilePosition != position + Vector2Int.left
                && changedTilePosition != position + Vector2Int.right)
                return;

            SetTexture();
        }

        private void SetTexture()
        {
            bool HasRoad(Vector2Int p) =>
                constructionGrid.RoadsTiles.Contains(p)
                || constructionGrid.RoadPreview.Contains(p);

            var up = HasRoad(position + Vector2Int.up);
            var down = HasRoad(position + Vector2Int.down);
            var left = HasRoad(position + Vector2Int.left);
            var right = HasRoad(position + Vector2Int.right);

            int connections =
                (up ? 1 : 0) +
                (down ? 1 : 0) +
                (left ? 1 : 0) +
                (right ? 1 : 0);

            if (connections == 4)
            {
                ApplySprite(GetSprite("RoadCrossroad"));
                return;
            }

            if (connections == 3)
            {
                if (!up) ApplySprite(GetSprite("RoadTDown"));
                else if (!down) ApplySprite(GetSprite("RoadTUp"));
                else if (!left) ApplySprite(GetSprite("RoadTRight"));
                else if (!right) ApplySprite(GetSprite("RoadTLeft"));

                return;
            }

            if (connections == 2)
            {
                if (up && down)
                {
                    ApplySprite(GetSprite("RoadVertical"));
                    return;
                }

                if (left && right)
                {
                    ApplySprite(GetSprite("RoadHorizontal"));
                    return;
                }

                if (up && right)
                {
                    ApplySprite(GetSprite("RoadRightTurn"));
                    return;
                }

                if (right && down)
                {
                    ApplySprite(GetSprite("RoadDownTurn"));
                    return;
                }

                if (down && left)
                {
                    ApplySprite(GetSprite("RoadLeftTurn"));
                    return;
                }

                if (left && up)
                {
                    ApplySprite(GetSprite("RoadUpTurn"));
                    return;
                }
            }

            if (connections == 1)
            {
                if (up) ApplySprite(GetSprite("RoadVertical"));
                else if (down) ApplySprite(GetSprite("RoadVertical"));
                else if (left) ApplySprite(GetSprite("RoadHorizontal"));
                else if (right) ApplySprite(GetSprite("RoadHorizontal"));

                return;
            }

            ApplySprite(GetSprite("RoadHorizontal"));
        }

        private Sprite GetSprite(string name)
        {
            if (!spriteCache.TryGetValue(name, out var sprite))
            {
                sprite = roadTextures.GetSprite(name);
                spriteCache[name] = sprite;
            }

            return sprite;
        }

        private void ApplySprite(Sprite sprite)
        {
            if (sprite == null || meshRenderer == null)
                return;

            var block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);

            var tex = sprite.texture;
            var rect = sprite.textureRect;

            var scale = new Vector2(
                rect.width / tex.width,
                rect.height / tex.height
            );

            var offset = new Vector2(
                rect.x / tex.width,
                rect.y / tex.height
            );

            block.SetTexture("_BaseMap", tex);
            block.SetVector("_BaseMap_ST", new Vector4(scale.x, scale.y, offset.x, offset.y));

            meshRenderer.SetPropertyBlock(block);
        }

        private void GenerateNavigationNodes(int resolution = 1)
        {
            var nodesPositions = new List<Vector3>();

            float step = 1f / resolution;
            float startOffset = 0.5f - step / 2f;

            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    float xPos = transform.position.x - startOffset + x * step;
                    float zPos = transform.position.z - startOffset + z * step;
                    float height = Terrain.activeTerrain.SampleHeight(new Vector3(xPos, 0, zPos)) + zFightOffset;

                    nodesPositions.Add(new Vector3(xPos, height, zPos));
                }
            }

            foreach (var position in nodesPositions)
            {
                navigationGraph.AddNode(position, NodeType.Road);
                navigationNodesForGizmos.Add(position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var nodePosition in navigationNodesForGizmos)
            {
                var node = navigationGraph.GetNode(nodePosition);
                Gizmos.DrawWireSphere(node.Data, .05f);

                foreach (var neighbor in node.Neighbors)
                {
                    Gizmos.DrawLine(nodePosition, neighbor.Data);
                }
            }
        }
    }
}