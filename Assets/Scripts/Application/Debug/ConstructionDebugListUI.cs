using UnityEngine;
using UnityEngine.UI;

namespace App.Debug
{
    public class ConstructionDebugListUI : MonoBehaviour
    {
        [SerializeField] private Toggle showGridToggle;
        [SerializeField] private Toggle showRoadsNodesToggle;
        [SerializeField] private Toggle showConstructionNodesToggle;
        [SerializeField] private Toggle showIrrigationNodesToggle;

        private GridRenderer gridRenderer;

        private void OnEnable()
        {
            gridRenderer ??= FindAnyObjectByType<GridRenderer>(FindObjectsInactive.Include);
            if (gridRenderer == null)
                return;

            showGridToggle.onValueChanged.AddListener((isOn) => gridRenderer.ShowGrid(isOn));
            showRoadsNodesToggle.onValueChanged.AddListener((isOn) => gridRenderer.showRoadsOccupation = isOn);
            showConstructionNodesToggle.onValueChanged.AddListener((isOn) => gridRenderer.showBuildingsOccupation = isOn);
            showIrrigationNodesToggle.onValueChanged.AddListener((isOn) => gridRenderer.showIrrigation = isOn);
        }

        private void OnDisable()
        {
            if (gridRenderer == null)
                return;

            showGridToggle.onValueChanged.RemoveAllListeners();
            showRoadsNodesToggle.onValueChanged.RemoveAllListeners();
            showConstructionNodesToggle.onValueChanged.RemoveAllListeners();
            showIrrigationNodesToggle.onValueChanged.RemoveAllListeners();
        }
    }
}