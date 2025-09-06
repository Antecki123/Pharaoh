using App.Helpers;
using App.Signals;
using Controllers;
using Controllers.Construction;
using Controllers.Settler;
using Models.Ai;
using Zenject;

namespace App.Registrators
{
    public class ApplicationRegistrator : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            new SignalInstaller(Container);

            // MODELS
            Container.Bind<NavigationGraph>().To<NavigationGraph>().AsSingle();
            Container.Bind<PrefabManager>().AsSingle();

            // CONTROLLERS
            Container.Bind(typeof(SettlersController), typeof(IInitializable), typeof(ITickable)).To<SettlersController>().AsSingle().NonLazy();
            Container.Bind(typeof(ConstructionController), typeof(IInitializable), typeof(ITickable)).To<ConstructionController>().AsSingle().NonLazy();
            Container.Bind(typeof(CameraController), typeof(IInitializable), typeof(ILateTickable)).To<CameraController>().AsSingle().NonLazy();

            new ApplicationInitializer();
        }
    }
}