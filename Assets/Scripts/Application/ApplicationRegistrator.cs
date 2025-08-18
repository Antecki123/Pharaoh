using App.Signals;
using Controllers.Settler;
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

            // CONTROLLERS
            Container.Bind(typeof(SettlersController), typeof(IInitializable), typeof(ITickable))
                .To<SettlersController>().AsSingle().NonLazy();


            new ApplicationInitializer();
        }
    }
}