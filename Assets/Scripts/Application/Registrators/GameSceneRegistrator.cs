using Controllers;
using Controllers.Ai.Strategy;
using Controllers.Construction;
using Controllers.Environment;
using Controllers.Gameplay;
using Controllers.Habitation;
using Controllers.Settler;
using Controllers.UI;
using Controllers.Work;
using Models.Ai;
using Models.Construction;
using Models.Economy;
using Models.Environment;
using Models.Gameplay;
using Models.Habitation;
using Models.Work;
using System;
using Views.Construction;
using Views.Settler;
using Zenject;

namespace App.Registrators
{
    public class GameSceneRegistrator : MonoInstaller
    {
        [Inject] private ApplicationRegistrator.SceneContextHolder contextHolder;

        public override void InstallBindings()
        {
            // MODELS
            Container.Bind<EconomyModel>().AsSingle();
            Container.Bind<HabitationModel>().AsSingle();
            Container.Bind<EmploymentModel>().AsSingle();
            Container.Bind<SupplyModel>().AsSingle();
            Container.Bind<NavigationGraph>().AsSingle().NonLazy();
            Container.Bind<ConstructionGrid>().AsSingle();
            Container.Bind<DateModel>().AsSingle();
            Container.Bind<IrrigationModel>().AsSingle();
            Container.Bind<ObjectivesModel>().AsSingle();

            // CONTROLLERS
            Container.Bind(typeof(ScenarioController), typeof(IInitializable), typeof(ITickable), typeof(IDisposable)).To<ScenarioController>().AsSingle().NonLazy();
            Container.Bind(typeof(ObjectivesController), typeof(IInitializable), typeof(ITickable)).To<ObjectivesController>().AsSingle().NonLazy();
            Container.Bind(typeof(SettlersController), typeof(IInitializable), typeof(ITickable), typeof(IDisposable)).To<SettlersController>().AsSingle().NonLazy();
            Container.Bind(typeof(WorkersController), typeof(IInitializable), typeof(ITickable), typeof(IDisposable)).To<WorkersController>().AsSingle().NonLazy();
            Container.Bind(typeof(WorkplacesController), typeof(IInitializable), typeof(ITickable), typeof(IDisposable)).To<WorkplacesController>().AsSingle().NonLazy();
            Container.Bind(typeof(HabitationController), typeof(IInitializable), typeof(ITickable)).To<HabitationController>().AsSingle().NonLazy();
            Container.Bind(typeof(InteractionController), typeof(IInitializable), typeof(ITickable), typeof(IDisposable)).To<InteractionController>().AsSingle().NonLazy();
            Container.Bind(typeof(ConstructionController), typeof(IInitializable), typeof(ITickable)).To<ConstructionController>().AsSingle().NonLazy();
            Container.Bind(typeof(CameraController), typeof(IInitializable), typeof(ILateTickable)).To<CameraController>().AsSingle().NonLazy();
            Container.Bind(typeof(EnvironmentController), typeof(ITickable)).To<EnvironmentController>().AsSingle().NonLazy();
            Container.Bind(typeof(BuildingsTooltipController), typeof(IInitializable), typeof(ITickable)).To<BuildingsTooltipController>().AsSingle().NonLazy();

            Container.BindFactory<RoadBuilder, RoadBuilder.Factory>().AsTransient();
            Container.BindFactory<ConstructionDestroyer, ConstructionDestroyer.Factory>().AsTransient();
            Container.BindFactory<ConstructionBuilder<BuildingView>, ConstructionBuilder<BuildingView>.Factory>().AsTransient();

            Container.Bind<StrategyFactory>().AsSingle();
            Container.BindFactory<SettlerView, SettlerStrategy, SettlerStrategy.Factory>().AsTransient();
            Container.BindFactory<SettlerView, ImmigrantStrategy, ImmigrantStrategy.Factory>().AsTransient();
            Container.BindFactory<SettlerView, RestingState, RestingState.Factory>().AsTransient();
            Container.BindFactory<SettlerView, WorkState, WorkState.Factory>().AsTransient();
            Container.BindFactory<SettlerView, LeisureState, LeisureState.Factory>().AsTransient();
            Container.BindFactory<SettlerView, PrayerState, PrayerState.Factory>().AsTransient();
            Container.BindFactory<SettlerView, HealingState, HealingState.Factory>().AsTransient();

            Container.BindFactory<ReachPopulationObjectiveDefinition, ReachPopulationObjective, ReachPopulationObjective.Factory>().AsTransient();
            Container.BindFactory<GatherGoldObjectiveDefinition, GatherGoldObjective, GatherGoldObjective.Factory>().AsTransient();
            Container.BindFactory<GatherCommodityObjectiveDefinition, GatherCommodityObjective, GatherCommodityObjective.Factory>().AsTransient();
            Container.BindFactory<BuildBuildingObjectiveDefinition, BuildBuildingObjective, BuildBuildingObjective.Factory>().AsTransient();

            contextHolder.Container = Container;
        }
    }
}