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

        public SceneHandler(SignalBus signalBus, PrefabManager prefabManager, ScenarioRepository scenarioRepository)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.scenarioRepository = scenarioRepository;

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
        }

        private async UniTask LoadScene(ApplicationSignals.LoadSceneRequest signal)
        {
            var oldScene = SceneManager.GetActiveScene();

            await SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);

            if (oldScene.IsValid())
            {
                await SceneManager.UnloadSceneAsync(oldScene);
            }

            await LoadPrefabs(signal.TargetScene);
            await LoadUiElements();

            Scene newScene;

            if (IsAddressableScene(signal.TargetScene))
            {
                var handle = Addressables.LoadSceneAsync(
                    signal.TargetScene,
                    LoadSceneMode.Additive,
                    activateOnLoad: false
                );

                await handle.Task;
                await handle.Result.ActivateAsync();
                newScene = handle.Result.Scene;
            }
            else
            {
                var loadOp = SceneManager.LoadSceneAsync(signal.TargetScene, LoadSceneMode.Additive);
                loadOp.allowSceneActivation = false;

                while (loadOp.progress < 0.9f)
                {
                    await UniTask.Yield();
                }

                loadOp.allowSceneActivation = true;
                await loadOp;

                newScene = SceneManager.GetSceneByName(signal.TargetScene);
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
            return scenarioRepository.Scenarios.Any(x => x.ScenarioName == sceneName);
        }
    }

    public enum SceneName
    {
        MainMenu,
        LoadingScene,
        Scenario01,
        Scenario02,
        Scenario03,
        Scenario04,
        Scenario05,
    }
}