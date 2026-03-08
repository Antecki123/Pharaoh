using App.Configs;
using App.Helpers;
using App.Signals;
using Controllers;
using Controllers.Construction;
using Controllers.Environment;
using Controllers.Habitation;
using Controllers.Settler;
using Controllers.UI;
using Controllers.Work;
using Models.Ai;
using Models.Construction;
using Models.Economy;
using Models.Environment;
using Models.Habitation;
using Models.Work;
using System;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace App.Registrators
{
    public class ApplicationRegistrator : MonoInstaller
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private ConstructionConfig constructionConfig;
        [SerializeField] private EnvironmentConfig environmentConfig;
        [Space]
        [SerializeField] private Canvas mainCanvas;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            new SignalInstaller(Container);

            // IMPORTERS
            Container.Bind<ConstructionDataImporter>().AsSingle();
            Container.Bind<SettlersNamesImporter>().AsSingle();
            Container.Bind<WorkplaceEconomyImporter>().AsSingle();

            // CONFIGS
            Container.Bind<GameConfig>().FromScriptableObject(gameConfig).AsSingle();
            Container.Bind<CameraConfig>().FromScriptableObject(cameraConfig).AsSingle();
            Container.Bind<ConstructionConfig>().FromScriptableObject(constructionConfig).AsSingle();
            Container.Bind<EnvironmentConfig>().FromScriptableObject(environmentConfig).AsSingle();

            Container.Bind<Canvas>().WithId("MainCanvas").FromComponentInNewPrefab(mainCanvas).AsSingle();

            // GLOBAL MODELS
            Container.Bind<EconomyModel>().AsSingle();
            Container.Bind<HabitationModel>().AsSingle();
            Container.Bind<EmploymentModel>().AsSingle();
            Container.Bind<SupplyModel>().AsSingle();
            Container.Bind<NavigationGraph>().AsSingle();
            Container.Bind<ConstructionGrid>().AsSingle();
            Container.Bind<PrefabManager>().AsSingle();
            Container.Bind<DateModel>().AsSingle();
            Container.Bind<IrrigationModel>().AsSingle();

            // CONTROLLERS
            Container.Bind(typeof(SettlersController), typeof(IInitializable), typeof(ITickable), typeof(IDisposable)).To<SettlersController>().AsSingle().NonLazy();
            Container.Bind(typeof(WorkersController), typeof(IInitializable), typeof(ITickable), typeof(IDisposable)).To<WorkersController>().AsSingle().NonLazy();
            Container.Bind(typeof(WorkplacesController), typeof(IInitializable), typeof(ITickable)).To<WorkplacesController>().AsSingle().NonLazy();
            Container.Bind(typeof(HabitationController), typeof(IInitializable), typeof(ITickable)).To<HabitationController>().AsSingle().NonLazy();
            Container.Bind(typeof(InteractionController), typeof(IInitializable), typeof(ITickable)).To<InteractionController>().AsSingle().NonLazy();
            Container.Bind(typeof(ConstructionController), typeof(IInitializable), typeof(ITickable)).To<ConstructionController>().AsSingle().NonLazy();
            Container.Bind(typeof(CameraController), typeof(IInitializable), typeof(ILateTickable)).To<CameraController>().AsSingle().NonLazy();
            Container.Bind(typeof(EnvironmentController), typeof(ITickable)).To<EnvironmentController>().AsSingle().NonLazy();
            Container.Bind(typeof(BuildingsTooltipController), typeof(IInitializable), typeof(ITickable)).To<BuildingsTooltipController>().AsSingle().NonLazy();

            Container.BindFactory<RoadBuilderRectangular, RoadBuilderRectangular.Factory>().AsTransient();
            Container.BindFactory<RoadBuilder, RoadBuilder.Factory>().AsTransient();
            Container.BindFactory<ConstructionBuilder<BuildingView>, ConstructionBuilder<BuildingView>.Factory>().AsTransient();

            Container.Bind(typeof(ApplicationInitializer), typeof(IInitializable)).To<ApplicationInitializer>().AsSingle();
        }
    }
}