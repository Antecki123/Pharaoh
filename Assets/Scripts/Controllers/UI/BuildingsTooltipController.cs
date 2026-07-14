using App.Helpers;
using App.Signals;
using System;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Controllers.UI
{
    public class BuildingsTooltipController : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;

        private Canvas mainCanvas;
        private BuildingTooltipUI currentTooltip;

        private bool interactionBlocked;

        public BuildingsTooltipController(SignalBus signalBus, PrefabManager prefabManager,
            [Inject(Id = "MainCanvas")] Canvas mainCanvas)
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
            signalBus.Subscribe<ConstructionSignals.ActivateConstructionMode>(OnConstructionModeChanged);
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1))
            {
                CloseTooltips();
            }
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<BuildingTooltipSignals.OpenHabitationTooltip>(OpenHabitationTooltip);
            signalBus.Unsubscribe<BuildingTooltipSignals.OpenProcessingWorkplaceTooltip>(OpenProcessingWorkplaceTooltip);
            signalBus.Unsubscribe<BuildingTooltipSignals.OpenStorageTooltipUI>(OpenStorageTooltipUI);
            signalBus.Unsubscribe<BuildingTooltipSignals.OpenDistributionPointTooltipUI>(OpenDistributionPointTooltipUI);
            signalBus.Unsubscribe<BuildingTooltipSignals.OpenFarmTooltipUI>(OpenFarmTooltipUI);
            signalBus.Unsubscribe<ConstructionSignals.ActivateConstructionMode>(OnConstructionModeChanged);
        }

        private void CloseTooltips()
        {
            if (currentTooltip != null)
                UnityEngine.Object.Destroy(currentTooltip.gameObject);

            currentTooltip = null;
        }

        private void OpenHabitationTooltip(BuildingTooltipSignals.OpenHabitationTooltip signal)
        {
            if (interactionBlocked)
                return;

            CloseTooltips();

            var tooltip = prefabManager.Instantiate<HabitationTooltipUI>("HabitationTooltipUI");
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenProcessingWorkplaceTooltip(BuildingTooltipSignals.OpenProcessingWorkplaceTooltip signal)
        {
            if (interactionBlocked)
                return;

            CloseTooltips();

            var tooltip = prefabManager.Instantiate<ProcessingWorkplaceTooltipUI>("ProcessingWorkplaceTooltipUI");
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenStorageTooltipUI(BuildingTooltipSignals.OpenStorageTooltipUI signal)
        {
            if (interactionBlocked)
                return;

            CloseTooltips();

            var tooltip = prefabManager.Instantiate<StorageTooltipUI>("StorageTooltipUI");
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenDistributionPointTooltipUI(BuildingTooltipSignals.OpenDistributionPointTooltipUI signal)
        {
            if (interactionBlocked)
                return;

            CloseTooltips();

            var tooltip = prefabManager.Instantiate<DistributionPointTooltipUI>("DistributionPointTooltipUI");
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OpenFarmTooltipUI(BuildingTooltipSignals.OpenFarmTooltipUI signal)
        {
            if (interactionBlocked)
                return;

            CloseTooltips();

            var tooltip = prefabManager.Instantiate<FarmTooltipUI>("FarmTooltipUI");
            tooltip.Init(signal.Transform, signal.Model);
            tooltip.gameObject.SetActive(true);
            currentTooltip = tooltip;

            tooltip.transform.SetParent(mainCanvas.transform);
        }

        private void OnConstructionModeChanged(ConstructionSignals.ActivateConstructionMode signal) =>
            interactionBlocked = signal.State;
    }
}