using App.Helpers;
using App.Signals;
using Cysharp.Threading.Tasks;
using Models.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Zenject;

namespace Controllers.SceneManagment
{
    public class SceneHandler : IInitializable, IDisposable
    {
        public event Action<string, float> OnAssetLoaded;

        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;
        private readonly ScenarioRepository scenarioRepository;
        private readonly ScenarioModel scenarioModel;

        private Dictionary<SceneName, Progression> sceneProgressionData = new Dictionary<SceneName, Progression>()
        {
            { SceneName.MainMenu, new Progression(0, 0) },
            { SceneName.ScenarioC01M01, new Progression(1, 1) },
            { SceneName.ScenarioC01M02, new Progression(1, 2) },
            { SceneName.ScenarioC01M03, new Progression(1, 3) },
            { SceneName.ScenarioC01M04, new Progression(1, 4) },
            { SceneName.ScenarioC01M05, new Progression(1, 5) }
        };

        public SceneHandler(SignalBus signalBus, PrefabManager prefabManager, ScenarioRepository scenarioRepository, ScenarioModel scenarioModel)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.scenarioRepository = scenarioRepository;
            this.scenarioModel = scenarioModel;

            SceneManager.sceneLoaded += (scene, loadMode) =>
            {
                Enum.TryParse(scene.name, out SceneName sceneName);
                signalBus.Fire(new ApplicationSignals.GameSceneLoaded(sceneName));
            };
        }

        public void Initialize()
        {
            signalBus.Subscribe<ApplicationSignals.LoadSceneRequest>(OnLoadSceneRequest);
        }

        public void Dispose()
        {
            signalBus.TryUnsubscribe<ApplicationSignals.LoadSceneRequest>(OnLoadSceneRequest);
        }

        private void OnLoadSceneRequest(ApplicationSignals.LoadSceneRequest signal)
        {
            LoadScene(signal).Forget(Debug.LogException);

            var currentScenario = scenarioRepository.Scenarios.FirstOrDefault(
                x => x.Chapter == sceneProgressionData[signal.TargetScene].Chapter
                && x.Mission == sceneProgressionData[signal.TargetScene].Mission);

            scenarioModel.SetupScenario(currentScenario);
        }

        private async UniTask LoadScene(ApplicationSignals.LoadSceneRequest signal)
        {
            var oldScene = SceneManager.GetActiveScene();

            await SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);

            if (oldScene.IsValid())
            {
                await SceneManager.UnloadSceneAsync(oldScene);
            }

            await LoadPrefabs(signal.TargetScene.ToString());
            await LoadUiElements();

            Scene newScene;

            if (IsAddressableScene(signal.TargetScene.ToString()))
            {
                var handle = Addressables.LoadSceneAsync(
                    signal.TargetScene.ToString(),
                    LoadSceneMode.Additive,
                    activateOnLoad: false
                );

                await handle.Task;
                await handle.Result.ActivateAsync();
                newScene = handle.Result.Scene;
            }
            else
            {
                var loadOp = SceneManager.LoadSceneAsync(signal.TargetScene.ToString(), LoadSceneMode.Additive);
                loadOp.allowSceneActivation = false;

                while (loadOp.progress < 0.9f)
                {
                    await UniTask.Yield();
                }

                loadOp.allowSceneActivation = true;
                await loadOp;

                newScene = SceneManager.GetSceneByName(signal.TargetScene.ToString());
            }

            SceneManager.SetActiveScene(newScene);

            var loadingScene = SceneManager.GetSceneByName("LoadingScene");
            if (loadingScene.IsValid() && loadingScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(loadingScene);
            }
        }

        private async UniTask LoadPrefabs(string sceneName)
        {
            var scenario = scenarioRepository.Scenarios.FirstOrDefault(x => x.ScenarioName == sceneName);

            if (scenario == null)
                return;

            var assetsToLoad = scenario.AvailableBuildings.Where(x => x.isAvailable).ToList();

            for (int i = 0; i < assetsToLoad.Count; i++)
            {
                await prefabManager.LoadGameObjectsAssets(assetsToLoad[i].buildingDefinition.ToString());
                OnAssetLoaded?.Invoke("LoadPrefabs", (float)(i + 1) / assetsToLoad.Count);
            }
        }

        private async UniTask LoadUiElements()
        {
            var assetsKeys = new List<string>()
            {
                "SettlerView",
                "CarrierView",
                "ServiceAgentView",

                // UI ELEMENTS
                "ProcessingWorkplaceTooltipUI",
                "HabitationTooltipUI",
                "StorageTooltipUI",
                "DistributionPointTooltipUI",
                "FarmTooltipUI"
            };

            for (int i = 0; i < assetsKeys.Count; i++)
            {
                await prefabManager.LoadGameObjectsAssets(assetsKeys[i]);
                OnAssetLoaded?.Invoke("LoadUiElements", (float)(i + 1) / assetsKeys.Count);
            }
        }

        private bool IsAddressableScene(string sceneName)
        {
            return scenarioRepository.Scenarios.Any(x => x.name == sceneName);
        }

        public struct Progression
        {
            public int Chapter;
            public int Mission;

            public Progression(int chapter, int mission)
            {
                Chapter = chapter;
                Mission = mission;
            }
        }
    }

    public enum SceneName
    {
        MainMenu,
        ScenarioC01M01,
        ScenarioC01M02,
        ScenarioC01M03,
        ScenarioC01M04,
        ScenarioC01M05,
    }
}