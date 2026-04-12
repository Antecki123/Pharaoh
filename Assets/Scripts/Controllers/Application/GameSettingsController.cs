using App.Helpers;
using App.Signals;
using Cysharp.Threading.Tasks;
using Models.Application;
using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Zenject;
using AudioSettings = Models.Application.AudioSettings;
using GraphicsSettings = Models.Application.GraphicsSettings;

namespace Controllers.Application
{
    public class GameSettingsController : IInitializable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly SettingsModel settingsModel;

        private FileDataHandler<SettingsModel> dataHandler;
        private SettingsModel cachedSettingsModel;

        public GameSettingsController(SignalBus signalBus, SettingsModel settingsModel)
        {
            this.signalBus = signalBus;
            this.settingsModel = settingsModel;

            cachedSettingsModel = new SettingsModel();
        }

        public void Initialize()
        {
            dataHandler = new FileDataHandler<SettingsModel>("settings.ini");

            if (dataHandler.FileExist())
                LoadGameSettings();
            else
                SetDefaultSettings();

            cachedSettingsModel = settingsModel.Clone();

            SubscribeSignals();
        }

        public void Dispose()
        {
            UnsubscribeSignals();
        }

        private void SetDefaultSettings()
        {
            settingsModel.GeneralSettings = new GeneralSettings()
            {
                Language = 1
            };

            settingsModel.ControlsSettings = new ControlsSettings()
            {

            };

            settingsModel.GraphicsSettings = new GraphicsSettings()
            {
                ResolutionWidth = 1920,
                ResolutionHeight = 1080,
                FullScreenMode = 1,
                RefreshRate = 1,
                VSync = 1,
                TargetFrameRate = 0,
                GraphicQuality = 1,
                AntiAliasing = 1,
                AnisotropicFiltering = 1,
                ShadowQuality = 1,
            };

            settingsModel.AudioSettings = new AudioSettings()
            {
                MasterVolume = 1f,
                SpeechVolume = 1f,
                EffectsVolume = 1f,
                Subtitles = 1,
                SubtitlesSize = 1,
            };

            dataHandler.Save(settingsModel);
        }

        private void LoadGameSettings(bool applySettings = true)
        {
            var data = dataHandler.Load();

            settingsModel.GeneralSettings = data.GeneralSettings.Clone();
            settingsModel.ControlsSettings = data.ControlsSettings.Clone();
            settingsModel.GraphicsSettings = data.GraphicsSettings.Clone();
            settingsModel.AudioSettings = data.AudioSettings.Clone();

            settingsModel.IsDirty = false;

            if (applySettings)
            {
                SetResolution(data.GraphicsSettings.ResolutionWidth, data.GraphicsSettings.ResolutionHeight);
                SetFullScreenMode(data.GraphicsSettings.FullScreenMode);
                SetRefreshRate(data.GraphicsSettings.RefreshRate);
                SetVSync(data.GraphicsSettings.VSync);
                SetTargetFrameRate(data.GraphicsSettings.TargetFrameRate);
                SetQualityLevel(data.GraphicsSettings.GraphicQuality);
                SetAntiAliasing(data.GraphicsSettings.AntiAliasing);
                SetAnisotropicFiltering(data.GraphicsSettings.AnisotropicFiltering);
                SetShadowsQuality(data.GraphicsSettings.ShadowQuality);

                SetMasterVolume(data.AudioSettings.MasterVolume);
                SetSpeechVolume(data.AudioSettings.SpeechVolume);
                SetEffectsVolume(data.AudioSettings.EffectsVolume);
                SetSubtitles(data.AudioSettings.Subtitles);
                SetSubtitlesSize(data.AudioSettings.SubtitlesSize);

                _ = SetLanguage(data.GeneralSettings.Language);
            }
        }

        private void SubscribeSignals()
        {
            signalBus.Subscribe<ApplicationSignals.SetResolution>(OnSetResolution);
            signalBus.Subscribe<ApplicationSignals.SetFullScreenMode>(OnSetFullScreenMode);
            signalBus.Subscribe<ApplicationSignals.SetRefreshRate>(OnSetRefreshRate);
            signalBus.Subscribe<ApplicationSignals.SetVSync>(OnSetVSync);
            signalBus.Subscribe<ApplicationSignals.SetTargetFrameRate>(OnSetTargetFrameRate);
            signalBus.Subscribe<ApplicationSignals.SetQualityLevel>(OnSetQualityLevel);
            signalBus.Subscribe<ApplicationSignals.SetAntiAliasing>(OnSetAntiAliasing);
            signalBus.Subscribe<ApplicationSignals.SetAnisotropicFiltering>(OnSetAnisotropicFiltering);
            signalBus.Subscribe<ApplicationSignals.SetShadowsQuality>(OnSetShadowsQuality);
            signalBus.Subscribe<ApplicationSignals.SetLanguage>(OnSetLanguage);
            signalBus.Subscribe<ApplicationSignals.SetMasterVolume>(OnSetMasterVolume);
            signalBus.Subscribe<ApplicationSignals.SetSpeechVolume>(OnSetSpeechVolume);
            signalBus.Subscribe<ApplicationSignals.SetEffectsVolume>(OnSetEffectsVolume);
            signalBus.Subscribe<ApplicationSignals.SetSubtitles>(OnSetSubtitles);
            signalBus.Subscribe<ApplicationSignals.SetSubtitlesSize>(OnSetSubtitlesSize);
        }

        private void UnsubscribeSignals()
        {
            signalBus.Unsubscribe<ApplicationSignals.SetResolution>(OnSetResolution);
            signalBus.Unsubscribe<ApplicationSignals.SetFullScreenMode>(OnSetFullScreenMode);
            signalBus.Unsubscribe<ApplicationSignals.SetRefreshRate>(OnSetRefreshRate);
            signalBus.Unsubscribe<ApplicationSignals.SetVSync>(OnSetVSync);
            signalBus.Unsubscribe<ApplicationSignals.SetTargetFrameRate>(OnSetTargetFrameRate);
            signalBus.Unsubscribe<ApplicationSignals.SetQualityLevel>(OnSetQualityLevel);
            signalBus.Unsubscribe<ApplicationSignals.SetAntiAliasing>(OnSetAntiAliasing);
            signalBus.Unsubscribe<ApplicationSignals.SetAnisotropicFiltering>(OnSetAnisotropicFiltering);
            signalBus.Unsubscribe<ApplicationSignals.SetShadowsQuality>(OnSetShadowsQuality);
            signalBus.Unsubscribe<ApplicationSignals.SetLanguage>(OnSetLanguage);
            signalBus.Unsubscribe<ApplicationSignals.SetMasterVolume>(OnSetMasterVolume);
            signalBus.Unsubscribe<ApplicationSignals.SetSpeechVolume>(OnSetSpeechVolume);
            signalBus.Unsubscribe<ApplicationSignals.SetEffectsVolume>(OnSetEffectsVolume);
            signalBus.Unsubscribe<ApplicationSignals.SetSubtitles>(OnSetSubtitles);
            signalBus.Unsubscribe<ApplicationSignals.SetSubtitlesSize>(OnSetSubtitlesSize);
        }

        private void OnSetResolution(ApplicationSignals.SetResolution signal) => SetResolution(signal.Width, signal.Height);
        private void OnSetFullScreenMode(ApplicationSignals.SetFullScreenMode signal) => SetFullScreenMode(signal.FullscreenMode);
        private void OnSetRefreshRate(ApplicationSignals.SetRefreshRate signal) => SetRefreshRate(signal.RefreshRate);
        private void OnSetVSync(ApplicationSignals.SetVSync signal) => SetVSync(signal.VSyncCount);
        private void OnSetTargetFrameRate(ApplicationSignals.SetTargetFrameRate signal) => SetTargetFrameRate(signal.TargetFrameRate);
        private void OnSetQualityLevel(ApplicationSignals.SetQualityLevel signal) => SetQualityLevel(signal.QualityLevel);
        private void OnSetAntiAliasing(ApplicationSignals.SetAntiAliasing signal) => SetAntiAliasing(signal.AntiAliasingLevel);
        private void OnSetAnisotropicFiltering(ApplicationSignals.SetAnisotropicFiltering signal) => SetAnisotropicFiltering(signal.Filter);
        private void OnSetShadowsQuality(ApplicationSignals.SetShadowsQuality signal) => SetShadowsQuality(signal.ShadowQuality);
        private void OnSetLanguage(ApplicationSignals.SetLanguage signal) => SetLanguage(signal.Locale).Forget();
        private void OnSetMasterVolume(ApplicationSignals.SetMasterVolume signal) => SetMasterVolume(signal.Volume);
        private void OnSetSpeechVolume(ApplicationSignals.SetSpeechVolume signal) => SetSpeechVolume(signal.Volume);
        private void OnSetEffectsVolume(ApplicationSignals.SetEffectsVolume signal) => SetEffectsVolume(signal.Volume);
        private void OnSetSubtitles(ApplicationSignals.SetSubtitles signal) => SetSubtitles(signal.Subtitles);
        private void OnSetSubtitlesSize(ApplicationSignals.SetSubtitlesSize signal) => SetSubtitlesSize(signal.SubtitlesSize);

        private void SetResolution(int width, int height)
        {
            Screen.SetResolution(width, height, Screen.fullScreenMode);

            settingsModel.GraphicsSettings.ResolutionWidth = width;
            settingsModel.GraphicsSettings.ResolutionHeight = height;

            settingsModel.IsDirty =
            settingsModel.GraphicsSettings.ResolutionWidth != cachedSettingsModel.GraphicsSettings.ResolutionWidth
            || settingsModel.GraphicsSettings.ResolutionHeight != cachedSettingsModel.GraphicsSettings.ResolutionHeight;

            dataHandler.Save(settingsModel);
        }

        public void SetFullScreenMode(int fullscreenMode)
        {
            Screen.fullScreenMode = (FullScreenMode)fullscreenMode;

            settingsModel.GraphicsSettings.FullScreenMode = fullscreenMode;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.FullScreenMode != cachedSettingsModel.GraphicsSettings.FullScreenMode;

            dataHandler.Save(settingsModel);
        }

        private void SetRefreshRate(uint refreshRate)
        {
            Screen.SetResolution(Screen.currentResolution.width,
                Screen.currentResolution.height, Screen.fullScreenMode,
                new RefreshRate()
                {
                    numerator = refreshRate,
                    denominator = 1
                });

            settingsModel.GraphicsSettings.RefreshRate = refreshRate;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.RefreshRate != cachedSettingsModel.GraphicsSettings.RefreshRate;

            dataHandler.Save(settingsModel);
        }

        private void SetVSync(int vSyncCount)
        {
            QualitySettings.vSyncCount = vSyncCount;

            settingsModel.GraphicsSettings.VSync = vSyncCount;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.VSync != cachedSettingsModel.GraphicsSettings.VSync;

            dataHandler.Save(settingsModel);
        }

        private void SetTargetFrameRate(int targetFrameRate)
        {
            UnityEngine.Application.targetFrameRate = targetFrameRate;

            settingsModel.GraphicsSettings.TargetFrameRate = targetFrameRate;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.TargetFrameRate != cachedSettingsModel.GraphicsSettings.TargetFrameRate;

            dataHandler.Save(settingsModel);
        }

        private void SetQualityLevel(int qualityLevel)
        {
            QualitySettings.SetQualityLevel(qualityLevel, true);

            settingsModel.GraphicsSettings.GraphicQuality = qualityLevel;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.GraphicQuality != cachedSettingsModel.GraphicsSettings.GraphicQuality;

            dataHandler.Save(settingsModel);
        }

        private void SetAntiAliasing(int antiAliasingLevel)
        {
            QualitySettings.antiAliasing = antiAliasingLevel;

            settingsModel.GraphicsSettings.AntiAliasing = antiAliasingLevel;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.AntiAliasing != cachedSettingsModel.GraphicsSettings.AntiAliasing;

            dataHandler.Save(settingsModel);
        }

        private void SetAnisotropicFiltering(int filter)
        {
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)filter;

            settingsModel.GraphicsSettings.AnisotropicFiltering = filter;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.AnisotropicFiltering != cachedSettingsModel.GraphicsSettings.AnisotropicFiltering;

            dataHandler.Save(settingsModel);
        }

        private void SetShadowsQuality(int shadowQuality)
        {
            QualitySettings.shadows = (ShadowQuality)shadowQuality;

            settingsModel.GraphicsSettings.ShadowQuality = shadowQuality;
            settingsModel.IsDirty =
                settingsModel.GraphicsSettings.ShadowQuality != cachedSettingsModel.GraphicsSettings.ShadowQuality;

            dataHandler.Save(settingsModel);
        }

        private async UniTask SetLanguage(int locale)
        {
            await LocalizationSettings.InitializationOperation.Task;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[locale];

            settingsModel.GeneralSettings.Language = locale;
            settingsModel.IsDirty = 
                settingsModel.GeneralSettings.Language != cachedSettingsModel.GeneralSettings.Language;

            dataHandler.Save(settingsModel);
        }

        private void SetMasterVolume(float volume)
        {
            settingsModel.AudioSettings.MasterVolume = volume;
            settingsModel.IsDirty =
                settingsModel.AudioSettings.MasterVolume != cachedSettingsModel.AudioSettings.MasterVolume;

            dataHandler.Save(settingsModel);
        }

        private void SetSpeechVolume(float volume)
        {
            settingsModel.AudioSettings.SpeechVolume = volume;
            settingsModel.IsDirty =
                settingsModel.AudioSettings.SpeechVolume != cachedSettingsModel.AudioSettings.SpeechVolume;

            dataHandler.Save(settingsModel);
        }

        private void SetEffectsVolume(float volume)
        {
            settingsModel.AudioSettings.EffectsVolume = volume;
            settingsModel.IsDirty =
                settingsModel.AudioSettings.EffectsVolume != cachedSettingsModel.AudioSettings.EffectsVolume;

            dataHandler.Save(settingsModel);
        }

        private void SetSubtitles(int subtitles)
        {
            settingsModel.AudioSettings.Subtitles = subtitles;
            settingsModel.IsDirty =
                settingsModel.AudioSettings.Subtitles != cachedSettingsModel.AudioSettings.Subtitles;

            dataHandler.Save(settingsModel);
        }

        private void SetSubtitlesSize(int subtitles)
        {
            settingsModel.AudioSettings.SubtitlesSize = subtitles;
            settingsModel.IsDirty =
                settingsModel.AudioSettings.SubtitlesSize != cachedSettingsModel.AudioSettings.SubtitlesSize;

            dataHandler.Save(settingsModel);
        }
    }
}