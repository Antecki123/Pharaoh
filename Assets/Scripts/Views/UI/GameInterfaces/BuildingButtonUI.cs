using App.Signals;
using Controllers.Construction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.GameInterfaces
{
    public class BuildingButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text buildingName;
        [SerializeField] private Image buildingSprite;
        [Space]
        [SerializeField] private Button selectConstructionButton;

        private SignalBus signalBus;
        private BuildingDefinition buildingDefinition;

        private void OnEnable()
        {
            selectConstructionButton.onClick.AddListener(OnSelectConstructionButton);
        }

        private void OnDisable()
        {
            selectConstructionButton.onClick.RemoveAllListeners();
        }

        private void OnSelectConstructionButton() => signalBus.Fire(new ConstructionSignals.ConstructionMode(buildingDefinition));

        public void InitializeButton(SignalBus signalBus, BuildingDefinition buildingDefinition)
        {
            this.signalBus = signalBus;
            this.buildingDefinition = buildingDefinition;

            buildingName.text = buildingDefinition.ToString();
        }
    }
}