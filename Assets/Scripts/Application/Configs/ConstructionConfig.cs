using UnityEngine;

namespace App.Configs
{
    [CreateAssetMenu(fileName = "ConstructionConfig", menuName = "Game Configs/ConstructionConfig")]
    public class ConstructionConfig : ScriptableObject
    {
        [field: Header("Constructions settings")]
        [field: SerializeField] public float MaxHeightDiff { get; private set; } = 1f;
    }
}