using App.Helpers;
using Controllers.SceneManagment;
using Models.Environment;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Zenject;

namespace Views.Visuals
{
    public class InfluenceOverlay : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter meshFilter;

        [Header("Gradients")]
        [SerializeField] private Gradient irrigationGradient;
        [SerializeField] private Gradient fireRiskGradient;
        [SerializeField] private Gradient aestheticsGradient;
        [SerializeField] private Gradient criminalGradient;

        [Header("Settings")]
        [SerializeField] private float terrainHeight = 0.5f;

        private static readonly int InfluenceTexID = Shader.PropertyToID("_InfluenceTex");
        private static readonly int GradientTexID = Shader.PropertyToID("_GradientTex");

        private static readonly string AssetSuffix = "_TerrainMesh";
        private static readonly int GridSize = 250;
        private static readonly int GradientLutSize = 256;

        private Texture2D influenceTex;
        private Texture2D gradientTex;

        private Gradient activeGradient;

        private InfluenceMap influenceMap;
        private PrefabManager prefabManager;

        private InfluenceType currentType;

        [Inject]
        public void Constructor(InfluenceMap influenceMap, PrefabManager prefabManager)
        {
            this.influenceMap = influenceMap;
            this.prefabManager = prefabManager;
        }

        private void Awake()
        {
            influenceTex = new Texture2D(GridSize, GridSize, TextureFormat.R8, mipChain: false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            influenceTex.Apply(updateMipmaps: false);

            gradientTex = new Texture2D(GradientLutSize, 1, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            gradientTex.Apply(updateMipmaps: false);

            var mat = meshRenderer.material;
            mat.SetTexture(InfluenceTexID, influenceTex);
            mat.SetTexture(GradientTexID, gradientTex);

            LoadMeshAsync();
        }

        private async void LoadMeshAsync()
        {
            var currentChapter = SceneHandler.GetCurrentChapterName();
            var mesh = await prefabManager.LoadObjectAsync<Mesh>($"{currentChapter}{AssetSuffix}");
            meshFilter.mesh = mesh;
        }

        private void Start()
        {
            BakeGradientLut(irrigationGradient);
            ShowOverlay(false);
        }

        private void OnEnable()
        {
            influenceMap.OnIrrigationInfluenceChanged += OnIrrigationInfluenceChanged;
            influenceMap.OnFireRiskInfluenceChanged += OnFireRiskInfluenceChanged;
            influenceMap.OnAestheticsInfluenceChanged += OnAestheticsInfluenceChanged;
            influenceMap.OnCriminalInfluenceChanged += OnCriminalInfluenceChanged;
        }

        private void OnDisable()
        {
            influenceMap.OnIrrigationInfluenceChanged -= OnIrrigationInfluenceChanged;
            influenceMap.OnFireRiskInfluenceChanged -= OnFireRiskInfluenceChanged;
            influenceMap.OnAestheticsInfluenceChanged -= OnAestheticsInfluenceChanged;
            influenceMap.OnCriminalInfluenceChanged -= OnCriminalInfluenceChanged;
        }

        private void OnDestroy()
        {
            if (influenceTex != null) Destroy(influenceTex);
            if (gradientTex != null) Destroy(gradientTex);
        }

        public void ShowOverlay(bool isOn, InfluenceType influenceType = default)
        {
            if (isOn)
            {
                activeGradient = GetGradient(influenceType);
                BakeGradientLut(activeGradient);
                RefreshInfluenceTex(influenceType);
            }

            meshRenderer.enabled = isOn;
        }

        private void OnIrrigationInfluenceChanged() => RefreshIfActive(InfluenceType.Irrigation);
        private void OnFireRiskInfluenceChanged() => RefreshIfActive(InfluenceType.FireRisk);
        private void OnAestheticsInfluenceChanged() => RefreshIfActive(InfluenceType.Aesthetics);
        private void OnCriminalInfluenceChanged() => RefreshIfActive(InfluenceType.Criminal);

        private void RefreshIfActive(InfluenceType type)
        {
            if (!meshRenderer.enabled || currentType != type) return;
            RefreshInfluenceTex(type);
        }

        private void RefreshInfluenceTex(InfluenceType influenceType)
        {
            if (influenceTex == null) return;

            currentType = influenceType;

            var influence = GetInfluenceData(influenceType);

            NativeArray<byte> raw = influenceTex.GetRawTextureData<byte>();

            for (int i = 0; i < raw.Length; i++)
                raw[i] = 0;

            if (influence == null || influence.Count == 0)
            {
                influenceTex.Apply(updateMipmaps: false);
                return;
            }

            foreach (KeyValuePair<Vector2Int, float> kvp in influence)
            {
                int x = kvp.Key.x;
                int y = kvp.Key.y;
                if ((uint)x >= GridSize || (uint)y >= GridSize) continue;

                raw[y * GridSize + x] = (byte)(Mathf.Clamp01(kvp.Value) * 255f);
            }

            influenceTex.Apply(updateMipmaps: false);
        }

        private void BakeGradientLut(Gradient gradient)
        {
            if (gradientTex == null || gradient == null) return;

            NativeArray<byte> raw = gradientTex.GetRawTextureData<byte>();

            for (int i = 0; i < GradientLutSize; i++)
            {
                float t = i / (float)(GradientLutSize - 1);
                Color col = gradient.Evaluate(t);

                int offset = i * 4;
                raw[offset + 0] = (byte)(col.r * 255f);
                raw[offset + 1] = (byte)(col.g * 255f);
                raw[offset + 2] = (byte)(col.b * 255f);
                raw[offset + 3] = (byte)(col.a * 255f);
            }

            gradientTex.Apply(updateMipmaps: false);
        }

        private Gradient GetGradient(InfluenceType type) => type switch
        {
            InfluenceType.Irrigation => irrigationGradient,
            InfluenceType.FireRisk => fireRiskGradient,
            InfluenceType.Aesthetics => aestheticsGradient,
            InfluenceType.Criminal => criminalGradient,
            _ => irrigationGradient
        };

        private IReadOnlyDictionary<Vector2Int, float> GetInfluenceData(InfluenceType type) => type switch
        {
            InfluenceType.Irrigation => influenceMap.IrrigationInfluence,
            InfluenceType.FireRisk => influenceMap.FireRiskInfluence,
            InfluenceType.Aesthetics => influenceMap.AestheticsInfluence,
            InfluenceType.Criminal => influenceMap.CriminalInfluence,
            _ => null
        };
    }

    public enum InfluenceType
    {
        Irrigation,
        FireRisk,
        Aesthetics,
        Criminal
    }
}