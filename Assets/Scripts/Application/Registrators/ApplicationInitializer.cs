using App.Helpers;
using App.Signals;
using Controllers.SceneManagment;
using Cysharp.Threading.Tasks;
using Models.Application;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace App.Registrators
{
    public class ApplicationInitializer : IInitializable
    {
        private readonly SignalBus signalBus;
        private readonly PrefabManager prefabManager;
        private readonly SettingsModel settingsModel;

        public ApplicationInitializer(SignalBus signalBus, PrefabManager prefabManager, SettingsModel settingsModel)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.settingsModel = settingsModel;
        }

        public async void Initialize()
        {
            signalBus.Subscribe<ApplicationSignals.GameSceneLoaded>(OnSceneLoaded);

            ApplyResolution();
            await LoadUiElements();
            //await InitializeAuthenticationService();

            signalBus.Fire(new ApplicationSignals.GameInitialized());
        }

        private void OnSceneLoaded(ApplicationSignals.GameSceneLoaded signal)
        {
            if (signal.SceneName == SceneName.MainMenu)
                signalBus.Fire(new ApplicationSignals.GameInitialized());
        }

        private async UniTask LoadUiElements()
        {
            var assetsKeys = new List<string>()
            {
                "WarningPanelUI"
            };

            for (int i = 0; i < assetsKeys.Count; i++)
            {
                await prefabManager.LoadGameObjectsAssets(assetsKeys[i]);
            }
        }

        //private async UniTask InitializeAuthenticationService()
        //{

        //}

        private void ApplyResolution()
        {
            var graphicsSettings = settingsModel.GraphicsSettings;

            if (IsResolutionSupported(graphicsSettings.ResolutionWidth, graphicsSettings.ResolutionHeight))
            {
                Screen.SetResolution(
                    graphicsSettings.ResolutionWidth,
                    graphicsSettings.ResolutionHeight,
                    (FullScreenMode)graphicsSettings.FullScreenMode);
            }
            else
            {
                var fallback = GetBestFallback();
                Screen.SetResolution(fallback.width, fallback.height, (FullScreenMode)graphicsSettings.FullScreenMode);
            }
        }

        private bool IsResolutionSupported(int w, int h)
        {
            return Screen.resolutions.Any(r => r.width == w && r.height == h);
        }

        private Resolution GetBestFallback()
        {
            return Screen.resolutions
                .OrderByDescending(r => r.width * r.height)
                .First();
        }
    }
}