using UnityEngine;
using UnityEngine.UI;
using Views.Visuals;
using Zenject;

namespace App.Debug
{
    public class ConstructionDebugListUI : MonoBehaviour
    {
        [SerializeField] private Toggle showGridToggle;
        [SerializeField] private Toggle showRoadsNodesToggle;
        [SerializeField] private Toggle showConstructionNodesToggle;

        private GridRenderer gridRenderer;

        [Inject]
        public void Constructor(GridRenderer gridRenderer)
        {
            this.gridRenderer = gridRenderer;
        }

        private void OnEnable()
        {
            showGridToggle.onValueChanged.AddListener(OnShowGrid);
            //showRoadsNodesToggle.onValueChanged.AddListener((isOn) => gridRenderer.showRoadsOccupation = isOn);
            //showConstructionNodesToggle.onValueChanged.AddListener((isOn) => gridRenderer.showBuildingsOccupation = isOn);
        }

        private void OnDisable()
        {
            showGridToggle.onValueChanged.RemoveAllListeners();
            showRoadsNodesToggle.onValueChanged.RemoveAllListeners();
            showConstructionNodesToggle.onValueChanged.RemoveAllListeners();
        }

        private void OnShowGrid(bool isOn) => gridRenderer.ShowGrid(isOn);
    }
}