using UnityEngine;

namespace App.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "Game Configs/EnvironmentConfig")]
    public class EnvironmentConfig : ScriptableObject
    {
        [field: Header("River settings")]
        [field: SerializeField] public float RiverRiseMinHeight { get; private set; } = 3f;
        [field: SerializeField] public float RiverRiseMaxHeight { get; private set; } = 6f;
        [field: SerializeField] public float RiverFallMinHeight { get; private set; } = .5f;
        [field: SerializeField] public float RiverFallMaxHeight { get; private set; } = 2.5f;

        [field: Header("Calendar settings")]
        [field: SerializeField, Tooltip("Duration of one month of real time in seconds")] public float MonthRealTimeDuration { get; private set; } = 5f;
    }
}