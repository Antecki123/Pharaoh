using App.Configs;
using App.Helpers;
using App.Signals;
using Controllers.Application;
using Controllers.Construction;
using Controllers.SceneManagment;
using Controllers.Settler;
using Controllers.Work;
using Models.Application;
using Models.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace App.Registrators
{
    public class ApplicationRegistrator : MonoInstaller
    {
        public class SceneContextHolder
        {
            public DiContainer Container;
        }

        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private ConstructionConfig constructionConfig;
        [SerializeField] private EnvironmentConfig environmentConfig;
        [Space]
        [SerializeField] private List<ScenarioData> scenariosData;
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
            Container.Bind<ScenarioRepository>().AsSingle().WithArguments(scenariosData);

            // MODELS
            Container.Bind<SettingsModel>().AsSingle();
            Container.Bind<ScenarioModel>().AsSingle();

            // CONFIGS
            Container.Bind<GameConfig>().FromScriptableObject(gameConfig).AsSingle();
            Container.Bind<CameraConfig>().FromScriptableObject(cameraConfig).AsSingle();
            Container.Bind<ConstructionConfig>().FromScriptableObject(constructionConfig).AsSingle();
            Container.Bind<EnvironmentConfig>().FromScriptableObject(environmentConfig).AsSingle();

            Container.Bind<SceneContextHolder>().AsSingle();
            Container.Bind<Canvas>().WithId("MainCanvas").FromComponentInNewPrefab(mainCanvas).AsSingle();

            // CONTROLLERS
            Container.Bind<PrefabManager>().AsSingle();
            Container.Bind(typeof(SceneHandler), typeof(IInitializable), typeof(IDisposable)).To<SceneHandler>().AsSingle().NonLazy();
            Container.Bind(typeof(GameSettingsController), typeof(IInitializable), typeof(IDisposable)).To<GameSettingsController>().AsSingle().NonLazy();

            Container.Bind(typeof(ApplicationInitializer), typeof(IInitializable)).To<ApplicationInitializer>().AsSingle().NonLazy();
        }
    }
}