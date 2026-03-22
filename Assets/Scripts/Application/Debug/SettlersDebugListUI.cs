using App.Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace App.Debug
{
    public class SettlersDebugListUI : MonoBehaviour
    {
        [SerializeField] private Button spawnSettlerBtn;
        [SerializeField] private Button spawnTenSettlersBtn;

        [Inject] private SignalBus signalBus;

        private void OnEnable()
        {
            spawnSettlerBtn.onClick.AddListener(() => SpawnSettlers(1));
            spawnTenSettlersBtn.onClick.AddListener(() => SpawnSettlers(10));
        }

        private void OnDisable()
        {
            spawnSettlerBtn.onClick.RemoveAllListeners();
            spawnTenSettlersBtn.onClick.RemoveAllListeners();
        }

        private void SpawnSettlers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                signalBus.Fire(new SettlersSignals.SpawnSettler(Vector3.zero, Quaternion.identity));
            }
        }
    }
}