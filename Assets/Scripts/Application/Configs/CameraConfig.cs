using System;
using UnityEngine;

namespace App.Configs
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Game Configs/CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Camera movement")]
        public float movementSpeed = 4f;
        public float acceleration = 10f;
        public ClampedValue cameraClampX = new(-5f, 255f); 
        public ClampedValue cameraClampZ = new(-5f, 255f); 

        [Header("Camera rotation")]
        public float sensitivity = 6f;
        public float rotationDamping = 20f;
        public ClampedValue cameraRotatnionY = new(10f, 70f);

        [Header("Camera scroll")]
        public float scrollSpeed = 50f;
        public float scrollDamping = 15f;
        public float minZoom = 15f;
        public float maxZoom = 60f;
    }

    [Serializable]
    public class ClampedValue
    {
        public float Min;
        public float Max;

        public ClampedValue(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}