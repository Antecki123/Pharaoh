using App.Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace App.Debug
{
    public class SettlersDebugListUI : MonoBehaviour
    {
        [SerializeField] private Button spawnSettlerBtn;

        [Inject] private SignalBus signalBus;

        private void OnEnable()
        {
            spawnSettlerBtn.onClick.AddListener(SpawnSettler);
        }

        private void OnDisable()
        {
            spawnSettlerBtn.onClick.RemoveAllListeners();
        }

        private void SpawnSettler()
        {
            signalBus.Fire(new SettlersSignals.SpawnSettler(Vector3.zero, Quaternion.identity));
        }
    }
}