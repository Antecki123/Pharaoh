using App.Signals;
using Controllers.SceneManagment;
using Zenject;

namespace App.Registrators
{
    public class ApplicationInitializer
    {
        [Inject] private SignalBus signalBus;

        public ApplicationInitializer(SignalBus signalBus)
        {
            this.signalBus = signalBus;

            signalBus.Subscribe<ApplicationSignals.GameSceneLoaded>(OnSceneLoaded);
        }

        private void OnSceneLoaded(ApplicationSignals.GameSceneLoaded signal)
        {
            if (signal.SceneName != SceneName.MainMenu)
                return;
        }
    }
}