using UnityEngine;

namespace App.Configs
{
    [CreateAssetMenu(fileName = "ConstructionConfig", menuName = "Game Configs/ConstructionConfig")]
    public class ConstructionConfig : ScriptableObject
    {
        [field: Header("Road settings")]
        [field: SerializeField] public float RoadWidth { get; private set; } = 3f;
        [field: SerializeField] public float SegmentSpacing { get; private set; } = 5f;
        [field: SerializeField] public float MinimumSpacing { get; private set; } = 1f;
        [field: SerializeField] public float MinimumRoadAngle { get; private set; } = 30f;

    }
}