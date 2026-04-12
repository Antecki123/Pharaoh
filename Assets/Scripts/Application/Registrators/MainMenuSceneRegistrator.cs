using Zenject;

namespace App.Registrators
{
    public class MainMenuSceneRegistrator : MonoInstaller
    {
        [Inject] private ApplicationRegistrator.SceneContextHolder contextHolder;

        public override void InstallBindings()
        {
            contextHolder.Container = Container;
        }
    }
}
