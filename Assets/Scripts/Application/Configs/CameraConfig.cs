using UnityEngine;

namespace App.Configs
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Game Configs/CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Camera movement")]
        public float movementSpeed = 4f;
        public float acceleration = 10f;

        [Header("Camera rotation")]
        public float sensitivity = 600f;
        public float rotationDamping = 20f;

        [Header("Camera scroll")]
        public float scrollSpeed = 50f;
        public float scrollDamping = 15f;
    }
}