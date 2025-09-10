using App.Configs;
using App.Helpers;
using App.Signals;
using Models.Ai;
using System;
using System.Collections.Generic;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionController : IInitializable, ITickable
    {
        private IConstruction currentConstruction;

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private NavigationGraph navigationGraph;
        private ConstructionConfig constructionConfig;
        private ConstructionDataImporter constructionDataImporter;

        private Dictionary<BuildingDefinition, Func<IConstruction>> constructionFactories;

        public ConstructionController(SignalBus signalBus, PrefabManager prefabManager, NavigationGraph navigationGraph, ConstructionConfig constructionConfig,
            ConstructionDataImporter constructionDataImporter)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.navigationGraph = navigationGraph;
            this.constructionConfig = constructionConfig;
            this.constructionDataImporter = constructionDataImporter;
        }

        public void Initialize()
        {
            constructionFactories = new Dictionary<BuildingDefinition, Func<IConstruction>>
            {
                { BuildingDefinition.None, () => null },
                { BuildingDefinition.Road, () => new RoadBuilder(signalBus, prefabManager, navigationGraph, constructionConfig) },
                { BuildingDefinition.Cottage, () => new ConstructionBuilder<CottageView>(signalBus, prefabManager, navigationGraph, constructionConfig, constructionDataImporter, BuildingDefinition.Cottage) },
                { BuildingDefinition.House, () => new ConstructionBuilder<HouseView>(signalBus, prefabManager, navigationGraph, constructionConfig, constructionDataImporter, BuildingDefinition.House) },
                { BuildingDefinition.FarmersHut, () => new ConstructionBuilder<FarmersHutView>(signalBus, prefabManager, navigationGraph, constructionConfig, constructionDataImporter, BuildingDefinition.FarmersHut) },
                { BuildingDefinition.WheatField, () => new ConstructionBuilder<WheatFieldView>(signalBus, prefabManager, navigationGraph, constructionConfig, constructionDataImporter, BuildingDefinition.WheatField) },
                { BuildingDefinition.LinenField, () => new ConstructionBuilder<LinenFieldView>(signalBus, prefabManager, navigationGraph, constructionConfig, constructionDataImporter, BuildingDefinition.LinenField) },
                { BuildingDefinition.Granary, () => new ConstructionBuilder<GranaryView>(signalBus, prefabManager, navigationGraph, constructionConfig, constructionDataImporter, BuildingDefinition.Granary) },
            };

            signalBus.Subscribe<ConstructionSignals.ConstructionMode>(SetConstruction);
        }

        public void Tick()
        {
            currentConstruction?.Tick();
        }

        private void SetConstruction(ConstructionSignals.ConstructionMode signal)
        {
            if (constructionFactories.TryGetValue(signal.Building, out var factory))
            {
                currentConstruction = factory?.Invoke();
                currentConstruction?.Initialize();
            }
        }
    }

    public enum BuildingDefinition
    {
        None,
        Road,
        Cottage,
        House,
        Residence,
        FarmersHut,
        WheatField,
        LinenField,
        Granary
    }
}
