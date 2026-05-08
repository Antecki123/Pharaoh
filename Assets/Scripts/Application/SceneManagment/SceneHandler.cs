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

        private readonly Dictionary<SceneName, SceneLoadType> sceneLoadingMethod = new()
        {
            { SceneName.MainMenu, SceneLoadType.Build },
            { SceneName.Chapter01, SceneLoadType.Addressable },
            { SceneName.Chapter02, SceneLoadType.Addressable },
            { SceneName.Chapter03, SceneLoadType.Addressable },
        };

        public SceneHandler(SignalBus signalBus, PrefabManager prefabManager, ScenarioRepository scenarioRepository, ScenarioModel scenarioModel)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.scenarioRepository = scenarioRepository;
            this.scenarioModel = scenarioModel;

            SceneManager.sceneLoaded += (scene, loadMode) =>
            {
                if (Enum.TryParse(scene.name, out SceneName sceneName))
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
            var currentScenario = scenarioRepository.Scenarios.FirstOrDefault(
                x => x.Scenario == signal.TargetScene
                && x.Mission == 1);
            scenarioModel.SetupScenario(currentScenario);

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

            await LoadPrefabs();
            await LoadUiElements();
            await LoadScene(signal.TargetScene);

            var loadingScene = SceneManager.GetSceneByName("LoadingScene");
            if (loadingScene.IsValid() && loadingScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(loadingScene);
            }
        }

        private async UniTask LoadPrefabs()
        {
            if (scenarioModel.Scenario == null)
                return;

            var assetsToLoad = scenarioModel.Scenario.AvailableBuildings.Where(x => x.isAvailable).ToList();

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
                "SelectionMaskView",

                // UI ELEMENTS
                "ProcessingWorkplaceTooltipUI",
                "HabitationTooltipUI",
                "StorageTooltipUI",
                "DistributionPointTooltipUI",
                "FarmTooltipUI",
                "WarningPanelUI"
            };

            for (int i = 0; i < assetsKeys.Count; i++)
            {
                await prefabManager.LoadGameObjectsAssets(assetsKeys[i]);
                OnAssetLoaded?.Invoke("LoadUiElements", (float)(i + 1) / assetsKeys.Count);
            }
        }

        private async UniTask LoadScene(SceneName targetScene)
        {
            Scene newScene;

            if (IsAddressableScene(targetScene))
            {
                var handle = Addressables.LoadSceneAsync(
                    targetScene.ToString(),
                    LoadSceneMode.Additive,
                    activateOnLoad: false
                );

                while (!handle.IsDone)
                {
                    var progress = handle.PercentComplete;
                    OnAssetLoaded?.Invoke("LoadingScene", progress);

                    await UniTask.Yield();
                }

                var activateHandle = handle.Result.ActivateAsync();
                while (!activateHandle.isDone)
                {
                    var progress = activateHandle.progress;
                    var normalized = Mathf.Clamp01(progress / 0.9f);
                    OnAssetLoaded?.Invoke("LoadingScene", normalized);

                    await UniTask.Yield();
                }

                newScene = handle.Result.Scene;
            }
            else
            {
                var loadOp = SceneManager.LoadSceneAsync(targetScene.ToString(), LoadSceneMode.Additive);
                loadOp.allowSceneActivation = false;

                while (loadOp.progress < 0.9f)
                {
                    OnAssetLoaded?.Invoke("LoadingScene", loadOp.progress);
                    await UniTask.Yield();
                }

                loadOp.allowSceneActivation = true;
                await loadOp;

                newScene = SceneManager.GetSceneByName(targetScene.ToString());
            }

            SceneManager.SetActiveScene(newScene);
        }

        private bool IsAddressableScene(SceneName sceneName)
        {
            return sceneLoadingMethod[sceneName] == SceneLoadType.Addressable;
        }
    }

    public enum SceneName
    {
        MainMenu,
        Chapter01,
        Chapter02,
        Chapter03
    }

    public enum SceneLoadType
    {
        Build,
        Addressable
    }
}