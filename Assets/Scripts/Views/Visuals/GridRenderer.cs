using App.Helpers;
using Controllers.SceneManagment;
using UnityEngine;
using Zenject;

namespace Views.Visuals
{
    public class GridRenderer : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter meshFilter;

        [Header("Settings")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private float edgeSoftness = 1f;

        private PrefabManager prefabManager;
        private const string assetSuffix = "_TerrainGrid";

        private Camera mainCamera;
        private Material material;

        private static readonly int CursorPos = Shader.PropertyToID("_CursorPos");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int EdgeSoft = Shader.PropertyToID("_EdgeSoftness");

        private const int layerMask = 1 << 16;
        private const float raycastDistance = 200f;

        [Inject]
        public void Constructor(PrefabManager prefabManager)
        {
            this.prefabManager = prefabManager;
        }

        private async void Awake()
        {
            mainCamera = Camera.main;

            var currentChapter = SceneHandler.GetCurrentChapterName();
            var mesh = await prefabManager.LoadObjectAsync<Mesh>($"{currentChapter}{assetSuffix}");
            meshFilter.mesh = mesh;

            material = meshRenderer.material;
            material.SetFloat(Radius, radius);
            material.SetFloat(EdgeSoft, edgeSoftness);
        }

        private void Start()
        {
            ShowGrid(false);
        }

        private void Update()
        {
            if (!meshRenderer.enabled || material == null)
                return;

            if (mainCamera == null)
                mainCamera = Camera.main;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, layerMask))
                material.SetVector(CursorPos, hit.point);
        }

        public void ShowGrid(bool isOn)
        {
            meshRenderer.enabled = isOn;
        }
    }
}