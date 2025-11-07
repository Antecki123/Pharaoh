using App.Signals;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Views.Irrigation
{
    public class RiverView : MonoBehaviour
    {
        [SerializeField] private GameObject waterSurface;
        private float waterSurfaceHeigh = 2f;

        [Inject] private SignalBus signalBus;

        private void OnEnable() => signalBus.Subscribe<EnvironmentSignals.RiverSurfaceHeightChanged>(SetWaterSurfaceHeight);

        private void OnDisable() => signalBus.TryUnsubscribe<EnvironmentSignals.RiverSurfaceHeightChanged>(SetWaterSurfaceHeight);

        private void SetWaterSurfaceHeight(EnvironmentSignals.RiverSurfaceHeightChanged signal)
        {
            waterSurfaceHeigh = signal.RiverSurfaceHeight;
            StartCoroutine(SetWaterSurfaceHeightCoroutine());
        }

        private IEnumerator SetWaterSurfaceHeightCoroutine()
        {
            var epsilon = 0.001f;
            var speed = 1f;

            while (Mathf.Abs(waterSurface.transform.position.y - waterSurfaceHeigh) > epsilon)
            {
                var direction = Mathf.Sign(waterSurfaceHeigh - waterSurface.transform.position.y);
                waterSurface.transform.position += direction * speed * Time.deltaTime * Vector3.up;

                yield return null;
            }

            var pos = waterSurface.transform.position;
            pos.y = waterSurfaceHeigh;
            waterSurface.transform.position = pos;
        }
    }
}