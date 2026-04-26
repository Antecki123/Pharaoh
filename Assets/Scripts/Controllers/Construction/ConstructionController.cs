using App.Signals;
using System;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Controllers.Construction
{
    public class ConstructionController : IInitializable, ITickable
    {
        private SignalBus signalBus;

        private IConstruction currentConstruction;

        private Dictionary<BuildingDefinition, Func<IConstruction>> constructionFactories;
        private Transform constructionsContainer;
        private Transform roadContainer;

        private RoadBuilder.Factory roadFactory;
        private ConstructionBuilder<BuildingView>.Factory constructionFactory;
        private ConstructionDestroyer.Factory destroyerFactory;

        public ConstructionController(SignalBus signalBus, RoadBuilder.Factory roadFactory,
            ConstructionBuilder<BuildingView>.Factory constructionFactory,
            ConstructionDestroyer.Factory destroyerFactory)
        {
            this.signalBus = signalBus;
            this.roadFactory = roadFactory;
            this.constructionFactory = constructionFactory;
            this.destroyerFactory = destroyerFactory;

            constructionsContainer = new GameObject("ConstructionsContainer").transform;
            roadContainer = new GameObject("RoadContainer").transform;
        }

        public void Initialize()
        {
            Func<IConstruction> BuildRoad()
            {
                var builder = roadFactory.Create();
                builder.Setup(roadContainer);
                return () => builder;
            }

            Func<IConstruction> Build<T>(BuildingDefinition def) where T : BuildingView
            {
                var builder = constructionFactory.Create();
                builder.Setup(def, constructionsContainer);
                return () => builder;
            }

            constructionFactories = new Dictionary<BuildingDefinition, Func<IConstruction>>
            {
                { BuildingDefinition.None, () => null },
                { BuildingDefinition.Road, BuildRoad() },
                { BuildingDefinition.Cottage, Build<CottageView>(BuildingDefinition.Cottage) },
                { BuildingDefinition.House, Build<HouseView>(BuildingDefinition.House) },
                { BuildingDefinition.Granary, Build<GranaryView>(BuildingDefinition.Granary) },
                { BuildingDefinition.Windmill, Build<WindmillView>(BuildingDefinition.Windmill) },
                { BuildingDefinition.Bakery, Build<BakeryView>(BuildingDefinition.Bakery) },
                { BuildingDefinition.Bazaar, Build<BazaarView>(BuildingDefinition.Bazaar) },
                { BuildingDefinition.Warehouse, Build<WarehouseView>(BuildingDefinition.Warehouse) },
                { BuildingDefinition.WheatFarm, Build<WheatFarmView>(BuildingDefinition.WheatFarm) },
                { BuildingDefinition.LinenFarm, Build<LinenFarmView>(BuildingDefinition.LinenFarm) },
                { BuildingDefinition.Pasture, Build<WarehouseView>(BuildingDefinition.Pasture) },
                { BuildingDefinition.Well, Build<WellView>(BuildingDefinition.Well) },
                { BuildingDefinition.Brewery, Build<BreweryView>(BuildingDefinition.Brewery) },
                { BuildingDefinition.WeavingMill, Build<WeavingMillView>(BuildingDefinition.WeavingMill) },
                { BuildingDefinition.Tavern, Build<TavernView>(BuildingDefinition.Tavern) },
            };

            signalBus.Subscribe<ConstructionSignals.ConstructionMode>(SetConstruction);
            signalBus.Subscribe<ConstructionSignals.DestroyMode>(SetDestroyMode);
        }

        public void Tick()
        {
            currentConstruction?.Tick();
        }

        private void SetConstruction(ConstructionSignals.ConstructionMode signal)
        {
            currentConstruction?.Dispose();

            if (constructionFactories.TryGetValue(signal.Building, out var factory))
            {
                currentConstruction = factory?.Invoke();
                currentConstruction?.Initialize();
            }
        }

        private void SetDestroyMode(ConstructionSignals.DestroyMode signal)
        {
            currentConstruction?.Dispose();
            currentConstruction = destroyerFactory.Create();
            currentConstruction?.Initialize();
        }
    }
}
