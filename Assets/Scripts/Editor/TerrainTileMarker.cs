using Models.Construction;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerrainTileMarker : EditorWindow
{
    private bool picking = false;
    private bool erasing = false;
    private TileType selectedType = TileType.Water;
    private List<TileData> tiles = new();

    private bool showGrid = true;
    private float cellSize = 1f;
    private int brushSize = 1;

    private Vector2Int? hoveredCell = null;

    private GameObject gridObject;
    private Mesh gridMesh;
    private Material gridMaterial;

    private Mesh tilesMesh;
    private GameObject tilesObject;
    private Material tilesMaterial;

    private bool tilesDirty = true;

    private static readonly Color ColorWater = new(0.15f, 0.55f, 1.00f, 0.72f);
    private static readonly Color ColorCliff = new(0.70f, 0.55f, 0.25f, 0.72f);
    private static readonly Color ColorBlocked = new(0.80f, 0.15f, 0.15f, 0.72f);
    private static readonly Color ColorGrid = new(1f, 1f, 1f, 0.18f);
    private static readonly Color ColorHover = new(1.00f, 1.00f, 0.20f, 0.35f);
    private static readonly Color ColorEraseHover = new(1.00f, 0.20f, 0.20f, 0.45f);

    [MenuItem("Tools/Terrain Tile Marker")]
    public static void ShowWindow()
    {
        var w = GetWindow<TerrainTileMarker>("Tile Marker");
        w.minSize = new Vector2(260, 300);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;

        LoadFromJson();

        CreateOrUpdateGrid();

        tilesDirty = true;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;

        EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;

        DestroyGrid();
        DestroyTilesMesh();
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        DestroyGrid();
        DestroyTilesMesh();

        LoadFromJson();

        CreateOrUpdateGrid();

        tilesDirty = true;

        SceneView.RepaintAll();
        Repaint();
    }

    private string GetSceneJsonPath()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string dir = $"Assets/Scenes/{sceneName}";

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        return $"{dir}/TerrainTiles.json";
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        var header = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.LabelField("Terrain Tile Marker", header);

        EditorGUILayout.Space(6);

        DrawLine();

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Picking", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = picking
            ? new Color(1f, 0.4f, 0.3f)
            : new Color(0.35f, 0.82f, 0.45f);

        if (GUILayout.Button(
                picking ? "⬛ Stop Picking" : "⬜ Start Picking",
                GUILayout.Height(34)))
        {
            picking = !picking;

            if (picking)
                erasing = false;

            SceneView.RepaintAll();
        }

        GUI.backgroundColor = erasing
            ? new Color(1f, 0.3f, 0.3f)
            : new Color(0.75f, 0.75f, 0.75f);

        if (GUILayout.Button(
                erasing ? "🗑 Stop Erase" : "🗑 Erase",
                GUILayout.Height(34)))
        {
            erasing = !erasing;

            if (erasing)
                picking = false;

            SceneView.RepaintAll();
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Tile Type", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        DrawTypeButton(TileType.Water, "Water", ColorWater);
        DrawTypeButton(TileType.Cliff, "Cliff", ColorCliff);
        DrawTypeButton(TileType.Blocked, "Blocked", ColorBlocked);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Size", GUILayout.Width(60));

        brushSize = EditorGUILayout.IntSlider(brushSize, 1, 10);

        EditorGUILayout.EndHorizontal();

        int diameter = brushSize * 2 - 1;

        EditorGUILayout.LabelField(
            $"{diameter}x{diameter} cells",
            EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);

        cellSize = Mathf.Max(
            0.5f,
            EditorGUILayout.FloatField("Cell Size", cellSize));

        if (EditorGUI.EndChangeCheck())
        {
            CreateOrUpdateGrid();

            tilesDirty = true;

            SceneView.RepaintAll();
        }

        DrawLine();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("💾 Save"))
            SaveToJson();

        if (GUILayout.Button("📂 Load"))
        {
            LoadFromJson();

            tilesDirty = true;

            RebuildTilesMesh();

            SceneView.RepaintAll();
            Repaint();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog(
                    "Clear All",
                    "Remove all tiles?",
                    "Yes",
                    "Cancel"))
            {
                tiles.Clear();

                tilesDirty = true;

                RebuildTilesMesh();

                SceneView.RepaintAll();
                Repaint();
            }
        }
    }

    private void SaveToJson()
    {
        string path = GetSceneJsonPath();

        string json = JsonUtility.ToJson(
            new TileDataCollection { tiles = tiles },
            true);

        File.WriteAllText(path, json);

        AssetDatabase.Refresh();

        Debug.Log($"Saved tiles to {path}");
    }

    private void LoadFromJson()
    {
        string path = GetSceneJsonPath();

        if (!File.Exists(path))
        {
            tiles.Clear();
            tilesDirty = true;
            return;
        }

        var wrapper = JsonUtility.FromJson<TileDataCollection>(
            File.ReadAllText(path));

        tiles = wrapper?.tiles ?? new List<TileData>();

        tilesDirty = true;

        Debug.Log($"Loaded tiles from {path}");
    }

    private void OnSceneGUI(SceneView sv)
    {
        var terrain = Terrain.activeTerrain;

        if (terrain == null)
            return;

        if (picking)
        {
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(FocusType.Passive));

            HandleHover(terrain, false);

            HandlePaint(terrain);

            sv.Repaint();
        }
        else if (erasing)
        {
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(FocusType.Passive));

            HandleHover(terrain, true);

            HandleErase(terrain);

            sv.Repaint();
        }

        if (tilesDirty)
            RebuildTilesMesh();
    }

    private void CreateOrUpdateGrid()
    {
        DestroyGrid();

        if (!showGrid)
            return;

        var terrain = Terrain.activeTerrain;

        if (terrain == null)
            return;

        gridObject = new GameObject("EditorGrid");
        gridObject.hideFlags = HideFlags.HideAndDontSave;

        var mf = gridObject.AddComponent<MeshFilter>();
        var mr = gridObject.AddComponent<MeshRenderer>();

        gridMaterial = CreateEditorMaterial(ColorGrid);

        mr.sharedMaterial = gridMaterial;

        gridMesh = GenerateGridMesh(terrain);

        mf.sharedMesh = gridMesh;
    }

    private Material CreateEditorMaterial(Color color)
    {
        Shader shader =
            Shader.Find("Hidden/Internal-Colored")
            ?? Shader.Find("Sprites/Default")
            ?? Shader.Find("Unlit/Color");

        var mat = new Material(shader);

        mat.hideFlags = HideFlags.HideAndDontSave;

        mat.color = color;

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        mat.SetInt("_ZWrite", 0);

        return mat;
    }

    private void DestroyGrid()
    {
        if (gridObject != null)
            DestroyImmediate(gridObject);

        if (gridMesh != null)
            DestroyImmediate(gridMesh);

        if (gridMaterial != null)
            DestroyImmediate(gridMaterial);

        gridObject = null;
        gridMesh = null;
        gridMaterial = null;
    }

    private Mesh GenerateGridMesh(Terrain terrain)
    {
        var mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        var td = terrain.terrainData;
        var origin = terrain.transform.position;

        int cols = Mathf.CeilToInt(td.size.x / cellSize);
        int rows = Mathf.CeilToInt(td.size.z / cellSize);

        var vertices = new List<Vector3>();
        var indices = new List<int>();

        float yOffset = 0.05f;

        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                float x0 = origin.x + x * cellSize;
                float z0 = origin.z + z * cellSize;

                float x1 = x0 + cellSize;
                float z1 = z0 + cellSize;

                Vector3 bl = new( x0, SampleY(terrain, x0, z0) + yOffset, z0);
                Vector3 tl = new( x0, SampleY(terrain, x0, z1) + yOffset, z1);
                Vector3 tr = new( x1, SampleY(terrain, x1, z1) + yOffset, z1);
                Vector3 br = new( x1, SampleY(terrain, x1, z0) + yOffset, z0);

                int i = vertices.Count;

                vertices.Add(bl);
                vertices.Add(tl);
                vertices.Add(tr);
                vertices.Add(br);

                indices.Add(i);
                indices.Add(i + 1);

                indices.Add(i + 1);
                indices.Add(i + 2);

                indices.Add(i + 2);
                indices.Add(i + 3);

                indices.Add(i + 3);
                indices.Add(i);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        mesh.RecalculateBounds();

        return mesh;
    }

    private void RebuildTilesMesh()
    {
        var terrain = Terrain.activeTerrain;

        if (terrain == null)
            return;

        if (tilesObject == null)
        {
            tilesObject = new GameObject("EditorTiles");
            tilesObject.hideFlags = HideFlags.HideAndDontSave;

            tilesObject.AddComponent<MeshFilter>();

            var mr = tilesObject.AddComponent<MeshRenderer>();

            tilesMaterial = CreateEditorMaterial(Color.white);

            mr.sharedMaterial = tilesMaterial;
        }

        if (tilesMesh == null)
        {
            tilesMesh = new Mesh();

            tilesMesh.indexFormat =
                UnityEngine.Rendering.IndexFormat.UInt32;

            tilesObject.GetComponent<MeshFilter>().sharedMesh =
                tilesMesh;
        }

        var vertices = new List<Vector3>(tiles.Count * 4);
        var colors = new List<Color>(tiles.Count * 4);
        var indices = new List<int>(tiles.Count * 6);

        var origin = terrain.transform.position;

        const float yOff = 0.8f;

        for (int t = 0; t < tiles.Count; t++)
        {
            var cell = tiles[t].cell;
            var color = TileColor(tiles[t].type);

            float x0 = origin.x + cell.x * cellSize;
            float z0 = origin.z + cell.y * cellSize;

            float x1 = x0 + cellSize;
            float z1 = z0 + cellSize;

            int i = vertices.Count;

            vertices.Add(new Vector3(
                x0,
                SampleY(terrain, x0, z0) + yOff,
                z0));

            vertices.Add(new Vector3(
                x1,
                SampleY(terrain, x1, z0) + yOff,
                z0));

            vertices.Add(new Vector3(
                x1,
                SampleY(terrain, x1, z1) + yOff,
                z1));

            vertices.Add(new Vector3(
                x0,
                SampleY(terrain, x0, z1) + yOff,
                z1));

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            indices.Add(i);
            indices.Add(i + 2);
            indices.Add(i + 1);

            indices.Add(i);
            indices.Add(i + 3);
            indices.Add(i + 2);
        }

        tilesMesh.Clear();

        tilesMesh.SetVertices(vertices);
        tilesMesh.SetColors(colors);
        tilesMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        tilesMesh.RecalculateBounds();

        tilesDirty = false;
    }

    private void DestroyTilesMesh()
    {
        if (tilesObject != null)
            DestroyImmediate(tilesObject);

        if (tilesMesh != null)
            DestroyImmediate(tilesMesh);

        if (tilesMaterial != null)
            DestroyImmediate(tilesMaterial);

        tilesObject = null;
        tilesMesh = null;
        tilesMaterial = null;
    }

    private IEnumerable<Vector2Int> BrushCells(Vector2Int center)
    {
        int r = brushSize - 1;

        for (int dz = -r; dz <= r; dz++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                yield return new Vector2Int(
                    center.x + dx,
                    center.y + dz);
            }
        }
    }

    private void HandleHover(Terrain terrain, bool isErasing)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(
            Event.current.mousePosition);

        if (!RaycastTerrain(terrain, ray, out var hit))
            return;

        hoveredCell = WorldToCell(hit);

        Color hoverColor = isErasing
            ? ColorEraseHover
            : ColorHover;

        foreach (var cell in BrushCells(hoveredCell.Value))
            DrawCellQuad(terrain, cell, hoverColor);
    }

    private bool IsMouseHeld()
    {
        var e = Event.current;

        return (e.type == EventType.MouseDown ||
                e.type == EventType.MouseDrag)
               && e.button == 0
               && !e.alt;
    }

    private void HandlePaint(Terrain terrain)
    {
        if (!IsMouseHeld())
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(
            Event.current.mousePosition);

        if (!RaycastTerrain(terrain, ray, out var hit))
            return;

        foreach (var cell in BrushCells(WorldToCell(hit)))
        {
            int idx = tiles.FindIndex(t => t.cell == cell);

            if (idx >= 0)
                tiles[idx].type = selectedType;
            else
                tiles.Add(new TileData(cell, selectedType));
        }

        tilesDirty = true;

        Event.current.Use();

        Repaint();
    }

    private void HandleErase(Terrain terrain)
    {
        if (!IsMouseHeld())
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(
            Event.current.mousePosition);

        if (!RaycastTerrain(terrain, ray, out var hit))
            return;

        foreach (var cell in BrushCells(WorldToCell(hit)))
            tiles.RemoveAll(t => t.cell == cell);

        tilesDirty = true;

        Event.current.Use();

        Repaint();
    }

    private void DrawCellQuad(Terrain terrain, Vector2Int cell, Color color)
    {
        var origin = terrain.transform.position;

        float x0 = origin.x + cell.x * cellSize;
        float z0 = origin.z + cell.y * cellSize;

        float x1 = x0 + cellSize;
        float z1 = z0 + cellSize;

        Vector3[] verts =
        {
            new(x0, SampleY(terrain, x0, z0) + 0.05f, z0),
            new(x1, SampleY(terrain, x1, z0) + 0.05f, z0),
            new(x1, SampleY(terrain, x1, z1) + 0.05f, z1),
            new(x0, SampleY(terrain, x0, z1) + 0.05f, z1),
        };

        Handles.DrawSolidRectangleWithOutline(
            verts,
            color,
            Color.white);
    }

    private bool RaycastTerrain(Terrain t, Ray ray, out Vector3 hit)
    {
        hit = Vector3.zero;

        var col = t.GetComponent<TerrainCollider>();

        if (col == null)
            return false;

        if (!col.Raycast(ray, out var info, 10000f))
            return false;

        hit = info.point;

        return true;
    }

    private Vector2Int WorldToCell(Vector3 pos)
    {
        var origin = Terrain.activeTerrain.transform.position;

        return new Vector2Int(
            Mathf.FloorToInt((pos.x - origin.x) / cellSize),
            Mathf.FloorToInt((pos.z - origin.z) / cellSize));
    }

    private float SampleY(Terrain t, float x, float z)
    {
        return t.SampleHeight(new Vector3(x, 0, z))
               + t.transform.position.y;
    }

    private Color TileColor(TileType t)
    {
        return t switch
        {
            TileType.Water => ColorWater,
            TileType.Cliff => ColorCliff,
            TileType.Blocked => ColorBlocked,
            _ => Color.white
        };
    }

    private void DrawTypeButton(
        TileType type,
        string label,
        Color col)
    {
        bool active = selectedType == type;

        GUI.backgroundColor = active
            ? col
            : new Color(col.r, col.g, col.b, 0.35f);

        var style = new GUIStyle(GUI.skin.button)
        {
            fontStyle = active
                ? FontStyle.Bold
                : FontStyle.Normal
        };

        if (GUILayout.Button(label, style, GUILayout.Height(28)))
        {
            selectedType = type;
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawLine()
    {
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f));
    }
}