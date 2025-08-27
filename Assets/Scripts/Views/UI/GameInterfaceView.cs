using App.Signals;
using Controllers.Construction;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui
{
    public class GameInterfaceView : MonoBehaviour
    {
        [SerializeField] private Button routeButton;

        [Inject] private SignalBus signalBus;

        private void OnEnable()
        {
            routeButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.Route)));
        }

        private void OnDisable()
        {
            routeButton.onClick.RemoveAllListeners();
        }
    }
}