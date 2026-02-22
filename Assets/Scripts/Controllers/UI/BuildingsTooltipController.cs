using App.Helpers;
using App.Signals;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Controllers.UI
{
    public class BuildingsTooltipController : IInitializable, ITickable
    {
        private SignalBus signalBus;
        private PrefabManager prefabManager;

        private Canvas mainCanvas;
        private BuildingTooltipUI currentTooltip;

        public BuildingsTooltipController(SignalBus signalBus, PrefabManager prefabManager, [Inject(Id = "MainCanvas")] Canvas mainCanvas)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.mainCanvas = mainCanvas;
        }

        public void Initialize()
        {
            signalBus.Subscribe<BuildingTooltipSignals.OpenHabitationTooltip>(OpenHabitationTooltip);
            signalBus.Subscribe<BuildingTooltipSignals.OpenProcessingWorkplaceTooltip>(OpenProcessingWorkplaceTooltip);
            signalBus.Subscribe<BuildingTooltipSignals.OpenStorageTooltipUI>(OpenStorageTooltipUI);
            signalBus.Subscribe<BuildingTooltipSignals.OpenDistributionPointTooltipUI>(OpenDistributionPointTooltipUI);
            signalBus.Subscribe<BuildingTooltipSignals.OpenFarmTooltipUI>(OpenFarmTooltipUI);
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                CloseTooltips();
            }
        }

        private void CloseTooltips()
        {
            if (currentTooltip != null)
                Object.Destroy(currentTooltip.gameObject);

            currentTooltip = null;
        }

        private void OpenHabitationTooltip(BuildingTooltipSignals.OpenHabitationTooltip signal)
        {
            CloseTooltips();

            var tooltip = prefabManager.Instantiate("HabitationTooltipUI").GetComponent<HabitationTooltipUI>();
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenProcessingWorkplaceTooltip(BuildingTooltipSignals.OpenProcessingWorkplaceTooltip signal)
        {
            CloseTooltips();

            var tooltip = prefabManager.Instantiate("ProcessingWorkplaceTooltipUI").GetComponent<ProcessingWorkplaceTooltipUI>();
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenStorageTooltipUI(BuildingTooltipSignals.OpenStorageTooltipUI signal)
        {
            CloseTooltips();

            var tooltip = prefabManager.Instantiate("StorageTooltipUI").GetComponent<StorageTooltipUI>();
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenDistributionPointTooltipUI(BuildingTooltipSignals.OpenDistributionPointTooltipUI signal)
        {
            CloseTooltips();

            var tooltip = prefabManager.Instantiate("DistributionPointTooltipUI").GetComponent<DistributionPointTooltipUI>();
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenFarmTooltipUI(BuildingTooltipSignals.OpenFarmTooltipUI signal)
        {
            CloseTooltips();

            var tooltip = prefabManager.Instantiate("FarmTooltipUI").GetComponent<FarmTooltipUI>();
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }
    }
}