using Controllers.Work;
using Models.Economy;
using Models.Work;
using UnityEngine;
using Zenject;

namespace App.Signals
{
    public class BuildingTooltipSignals
    {
        public BuildingTooltipSignals(DiContainer container)
        {
            container.DeclareSignal<OpenHabitationTooltip>();
            container.DeclareSignal<OpenProcessingWorkplaceTooltip>();
            container.DeclareSignal<OpenStorageTooltipUI>();
            container.DeclareSignal<OpenDistributionPointTooltipUI>();
            container.DeclareSignal<OpenFarmTooltipUI>();
        }

        public class OpenHabitationTooltip
        {
            public Transform Transform { get; private set; }

            public HabitatModel Model { get; private set; }

            public OpenHabitationTooltip(Transform transform, HabitatModel model)
            {
                Transform = transform;
                Model = model;
            }
        }

        public class OpenProcessingWorkplaceTooltip
        {
            public Transform Transform { get; private set; }

            public WorkplaceModel Model { get; private set; }

            public OpenProcessingWorkplaceTooltip(Transform transform, WorkplaceModel model)
            {
                Transform = transform;
                Model = model;
            }
        }

        public class OpenStorageTooltipUI
        {
            public Transform Transform { get; private set; }

            public StorageModel Model { get; private set; }

            public OpenStorageTooltipUI(Transform transform, StorageModel model)
            {
                Transform = transform;
                Model = model;
            }
        }

        public class OpenDistributionPointTooltipUI
        {
            public Transform Transform { get; private set; }

            public DistributionPointModel Model { get; private set; }

            public OpenDistributionPointTooltipUI(Transform transform, DistributionPointModel model)
            {
                Transform = transform;
                Model = model;
            }
        }

        public class OpenFarmTooltipUI
        {
            public Transform Transform { get; private set; }

            public FarmWorkplaceModel Model { get; private set; }

            public OpenFarmTooltipUI(Transform transform, FarmWorkplaceModel model)
            {
                Transform = transform;
                Model = model;
            }
        }
    }
}