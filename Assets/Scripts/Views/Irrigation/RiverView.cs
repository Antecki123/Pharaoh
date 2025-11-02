using UnityEngine;

namespace Views.Irrigation
{
    public class RiverView : MonoBehaviour
    {
        [SerializeField] private GameObject waterSurface;
        [SerializeField, Range(1.5f, 6f)] private float waterSurfaceHeigh = 4.5f;

        private void Update()
        {
            waterSurface.transform.position = new Vector3(waterSurface.transform.position.x, waterSurfaceHeigh, waterSurface.transform.position.z);
        }
    }
}