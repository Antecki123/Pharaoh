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
        [SerializeField] private Button cottageButton;
        [SerializeField] private Button houseButton;
        [SerializeField] private Button farmersHutButton;
        [SerializeField] private Button wheatFieldButton;
        [SerializeField] private Button linenFieldButton;
        [SerializeField] private Button granaryButton;

        [Inject] private SignalBus signalBus;

        private void OnEnable()
        {
            routeButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.Road)));
            cottageButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.Cottage)));
            houseButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.House)));
            farmersHutButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.FarmersHut)));
            wheatFieldButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.WheatField)));
            linenFieldButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.LinenField)));
            granaryButton.onClick.AddListener(() => signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.Granary)));
        }

        private void OnDisable()
        {
            routeButton.onClick.RemoveAllListeners();
            cottageButton.onClick.RemoveAllListeners();
            houseButton.onClick.RemoveAllListeners();
            farmersHutButton.onClick.RemoveAllListeners();
            wheatFieldButton.onClick.RemoveAllListeners();
            linenFieldButton.onClick.RemoveAllListeners();
            granaryButton.onClick.RemoveAllListeners();
        }
    }
}