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
        private FarmBuilder.Factory farmFactory;
        private ConstructionBuilder<BuildingView>.Factory constructionFactory;

        public ConstructionController(SignalBus signalBus, RoadBuilder.Factory roadFactory, FarmBuilder.Factory farmFactory,
            ConstructionBuilder<BuildingView>.Factory constructionFactory)
        {
            this.signalBus = signalBus;
            this.roadFactory = roadFactory;
            this.farmFactory = farmFactory;
            this.constructionFactory = constructionFactory;

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

            Func<IConstruction> BuildFarm(BuildingDefinition def)
            {
                var builder = farmFactory.Create();
                builder.Setup(def, constructionsContainer);
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
                { BuildingDefinition.WheatField, BuildFarm(BuildingDefinition.WheatFarm) },
                { BuildingDefinition.LinenField, BuildFarm(BuildingDefinition.LinenFarm) },
                { BuildingDefinition.Pasture, BuildFarm(BuildingDefinition.Pasture) },
                { BuildingDefinition.Cottage, Build<CottageView>(BuildingDefinition.Cottage) },
                { BuildingDefinition.House, Build<HouseView>(BuildingDefinition.House) },
                { BuildingDefinition.Granary, Build<GranaryView>(BuildingDefinition.Granary) },
                { BuildingDefinition.Windmill, Build<WindmillView>(BuildingDefinition.Windmill) },
                { BuildingDefinition.Bakery, Build<BakeryView>(BuildingDefinition.Bakery) },
                { BuildingDefinition.Bazaar, Build<BazaarView>(BuildingDefinition.Bazaar) },
                { BuildingDefinition.Warehouse, Build<WarehouseView>(BuildingDefinition.Warehouse) },
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
        [Obsolete] FarmersHut,
        WheatField,
        LinenField,
        Granary,
        Windmill,
        Bakery,
        Bazaar,
        Warehouse,
        WheatFarm,
        LinenFarm,
        Pasture
    }
}
