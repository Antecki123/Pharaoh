using UnityEngine;

namespace App.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game Configs/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [field: SerializeField] public bool CheatsEnabled { get; private set; } = false;
        [field: SerializeField] public bool DebugEnabled { get; private set; } = false;
    }
}