using Controllers.SceneManagment;
using Zenject;

namespace App.Signals
{
    public class ApplicationSignals
    {
        public ApplicationSignals(DiContainer container)
        {
            container.DeclareSignal<LoadSceneRequest>();
            container.DeclareSignal<GameSceneLoaded>();
        }

        public class LoadSceneRequest
        {
            public SceneName TargetScene { get; private set; }

            public LoadSceneRequest(SceneName targetScene)
            {
                TargetScene = targetScene;
            }
        }

        public class GameSceneLoaded
        {
            public SceneName SceneName { get; private set; }

            public GameSceneLoaded(SceneName sceneName)
            {
                SceneName = sceneName;
            }
        }
    }
}