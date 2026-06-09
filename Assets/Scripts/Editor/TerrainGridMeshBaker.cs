using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class TerrainGridMeshBaker : EditorWindow
{
    [MenuItem("Tools/Bake Terrain Grid Mesh")]
    public static void ShowWindow()
    {
        GetWindow<TerrainGridMeshBaker>("Terrain Grid Baker");
    }

    private Terrain sourceTerrain;
    private int gridResolution = 250;
    private float gridHeightOffset = 0.05f;
    private string saveFolder = "Assets/Meshes";

    private const string ChapterPrefix = "Chapter";

    private void OnEnable()
    {
        EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
        TryAssignMainTerrain();
    }

    private void OnDisable()
    {
        EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
    }

    private void OnFocus()
    {
        TryAssignMainTerrain();
    }

    private void OnSceneChanged(Scene previous, Scene next)
    {
        TryAssignMainTerrain();
        Repaint();
    }

    /// <summary>
    /// Finds a Terrain whose GameObject has the "MainTerrain" tag and assigns
    /// it to <see cref="sourceTerrain"/>. Does nothing if already assigned to a
    /// valid object in the current scene or if no such tag exists.
    /// </summary>
    private void TryAssignMainTerrain()
    {
        if (sourceTerrain != null)
            return;

        try
        {
            GameObject tagged = GameObject.FindGameObjectWithTag("MainTerrain");
            if (tagged != null)
                sourceTerrain = tagged.GetComponent<Terrain>();
        }
        catch (UnityException)
        {
            // Tag "MainTerrain" is not defined in this project — silently ignore.
        }
    }


    private static bool IsScenarioScene() =>
        SceneManager.GetActiveScene().name.StartsWith(ChapterPrefix);

    private string GetSceneMeshName(string suffix)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneName))
            sceneName = "Untitled";
        return sceneName + suffix;
    }

    void OnGUI()
    {
        EditorGUILayout.Space(10);

        var header = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.LabelField("Terrain Grid Mesh Baker", header);

        if (!IsScenarioScene())
        {
            EditorGUILayout.Space(6);

            var warningStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField(
                $"⚠ This scene is not a chapter (name must start with \"{ChapterPrefix}\").\n" +
                "Saving and loading tiles is disabled.",
                warningStyle,
                GUILayout.Height(42));

            DrawLine();
            return;
        }

        DrawLine();

        EditorGUILayout.Space(8);

        sourceTerrain = (Terrain)EditorGUILayout.ObjectField("Terrain", sourceTerrain, typeof(Terrain), true);
        gridResolution = EditorGUILayout.IntSlider("Grid Resolution", gridResolution, 10, 512);
        gridHeightOffset = EditorGUILayout.FloatField("Height Offset", gridHeightOffset);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        EditorGUILayout.Space(4);

        string gridMeshName = GetSceneMeshName("_TerrainGrid");
        EditorGUILayout.HelpBox(
            $"[Grid] {saveFolder}/{gridMeshName}.asset\n" +
            $"Resolution: {gridResolution}×{gridResolution}   " +
            $"Vertices: {(gridResolution + 1) * (gridResolution + 1)}",
            MessageType.Info);

        if (GUILayout.Button("Bake Grid Mesh") && sourceTerrain != null)
            BakeTerrainGridMesh();

        EditorGUILayout.Space(6);
        DrawLine();
        EditorGUILayout.Space(6);

        string fullMeshName = GetSceneMeshName("_TerrainMesh");
        int fullVerts = (gridResolution + 1) * (gridResolution + 1);
        int fullTris = gridResolution * gridResolution * 2;
        EditorGUILayout.HelpBox(
            $"[Full] {saveFolder}/{fullMeshName}.asset\n" +
            $"Resolution: {gridResolution}×{gridResolution}   " +
            $"Vertices: {fullVerts}   Triangles: {fullTris}",
            MessageType.Info);

        if (GUILayout.Button("Bake Full Terrain Mesh") && sourceTerrain != null)
            BakeFullTerrainMesh();
    }

    private void BakeTerrainGridMesh()
    {
        TerrainData td = sourceTerrain.terrainData;
        Vector3 terrainSize = td.size;
        int res = gridResolution;
        int vertCount = (res + 1) * (res + 1);

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        for (int z = 0; z <= res; z++)
            for (int x = 0; x <= res; x++)
            {
                float normX = (float)x / res;
                float normZ = (float)z / res;
                float height = td.GetInterpolatedHeight(normX, normZ);

                int i = z * (res + 1) + x;
                vertices[i] = new Vector3(normX * terrainSize.x, height + gridHeightOffset, normZ * terrainSize.z);
                uvs[i] = new Vector2(normX, normZ);
            }

        int hLines = (res + 1) * res;
        int vLines = res * (res + 1);
        int[] indices = new int[(hLines + vLines) * 2];
        int idx = 0;

        for (int z = 0; z <= res; z++)
            for (int x = 0; x < res; x++)
            {
                indices[idx++] = z * (res + 1) + x;
                indices[idx++] = z * (res + 1) + x + 1;
            }

        for (int x = 0; x <= res; x++)
            for (int z = 0; z < res; z++)
            {
                indices[idx++] = z * (res + 1) + x;
                indices[idx++] = (z + 1) * (res + 1) + x;
            }

        string meshName = GetSceneMeshName("_TerrainGrid");
        Mesh mesh = BuildMesh(meshName, vertices, uvs, vertCount);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        SaveMesh(mesh, meshName);
    }

    private void BakeFullTerrainMesh()
    {
        TerrainData td = sourceTerrain.terrainData;
        Vector3 terrainSize = td.size;
        int res = gridResolution;
        int vertCount = (res + 1) * (res + 1);

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        for (int z = 0; z <= res; z++)
            for (int x = 0; x <= res; x++)
            {
                float normX = (float)x / res;
                float normZ = (float)z / res;
                float height = td.GetInterpolatedHeight(normX, normZ);

                int i = z * (res + 1) + x;
                vertices[i] = new Vector3(normX * terrainSize.x, height, normZ * terrainSize.z);
                uvs[i] = new Vector2(normX, normZ);
            }

        int[] triangles = new int[res * res * 6];
        int idx = 0;

        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
            {
                int bl = z * (res + 1) + x;
                int br = bl + 1;
                int tl = (z + 1) * (res + 1) + x;
                int tr = tl + 1;

                triangles[idx++] = bl;
                triangles[idx++] = tl;
                triangles[idx++] = tr;

                triangles[idx++] = bl;
                triangles[idx++] = tr;
                triangles[idx++] = br;
            }

        string meshName = GetSceneMeshName("_TerrainMesh");
        Mesh mesh = BuildMesh(meshName, vertices, uvs, vertCount);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        SaveMesh(mesh, meshName);
    }

    private static Mesh BuildMesh(string meshName, Vector3[] vertices, Vector2[] uvs, int vertCount)
    {
        Mesh mesh = new Mesh { name = meshName };

        if (vertCount > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.uv = uvs;
        return mesh;
    }

    private void SaveMesh(Mesh mesh, string meshName)
    {
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string savePath = $"{saveFolder}/{meshName}.asset";
        AssetDatabase.CreateAsset(mesh, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[TerrainGridMeshBaker] Saved: {savePath}  |  Vertices: {mesh.vertexCount}");
    }

    private void DrawLine()
    {
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f));
    }
}