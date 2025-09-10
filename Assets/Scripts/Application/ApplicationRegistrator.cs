using App.Configs;
using App.Helpers;
using App.Signals;
using Controllers;
using Controllers.Calendar;
using Controllers.Construction;
using Controllers.Settler;
using Models.Ai;
using Models.Economy;
using UnityEngine;
using Zenject;

namespace App.Registrators
{
    public class ApplicationRegistrator : MonoInstaller
    {
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private ConstructionConfig constructionConfig;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            new SignalInstaller(Container);

            // IMPORTERS
            Container.Bind<ConstructionDataImporter>().To<ConstructionDataImporter>().AsSingle();

            // CONFIGS
            Container.Bind<CameraConfig>().FromScriptableObject(cameraConfig).AsSingle();
            Container.Bind<ConstructionConfig>().FromScriptableObject(constructionConfig).AsSingle();

            // MODELS
            Container.Bind<EconomyModel>().To<EconomyModel>().AsSingle();
            Container.Bind<HabitationModel>().To<HabitationModel>().AsSingle();
            Container.Bind<NavigationGraph>().To<NavigationGraph>().AsSingle();
            Container.Bind<PrefabManager>().To<PrefabManager>().AsSingle();

            // CONTROLLERS
            Container.Bind(typeof(SettlersController), typeof(IInitializable), typeof(ITickable)).To<SettlersController>().AsSingle().NonLazy();
            Container.Bind(typeof(ConstructionController), typeof(IInitializable), typeof(ITickable)).To<ConstructionController>().AsSingle().NonLazy();
            Container.Bind(typeof(CameraController), typeof(IInitializable), typeof(ILateTickable)).To<CameraController>().AsSingle().NonLazy();
            Container.Bind(typeof(CalendarController), typeof(ITickable)).To<CalendarController>().AsSingle().NonLazy();

            new ApplicationInitializer();
        }
    }
}