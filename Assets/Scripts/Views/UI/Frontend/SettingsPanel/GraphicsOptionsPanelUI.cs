using App.Signals;
using Models.Application;
using UnityEngine;
using Zenject;

namespace Views.Ui.Frontend
{
    public class GraphicsOptionsPanelUI : OptionsPanel
    {
        [SerializeField] private ResolutionOptionDropdownUI resolutionDropdown;
        [SerializeField] private OptionDropdownElementUI fullscreenDropdown;
        [SerializeField] private OptionToggleElementUI vSyncToggle;
        [SerializeField] private OptionDropdownElementUI graphicQualityDropdown;
        [SerializeField] private OptionDropdownElementUI antiAliasingDropdown;
        [SerializeField] private OptionDropdownElementUI anisotropicFilteringDropdown;
        [SerializeField] private OptionDropdownElementUI shadowQualityDropdown;

        private SignalBus signalBus;
        private SettingsModel settingsModel;

        [Inject]
        public void Constructor(SignalBus signalBus, SettingsModel settingsModel)
        {
            this.signalBus = signalBus;
            this.settingsModel = settingsModel;
        }

        private void OnEnable()
        {
            resolutionDropdown.OnValueChanged += OnResolutionValueChanged;
            fullscreenDropdown.OnValueChanged += OnFullscreenValueChanged;
            vSyncToggle.OnValueChanged += OnVSyncToggleValueChanged;
            graphicQualityDropdown.OnValueChanged += OnQualityLevelValueChanged;
            antiAliasingDropdown.OnValueChanged += OnAntiAliasingValueChanged;
            anisotropicFilteringDropdown.OnValueChanged += OnAnisotropicFilteringValueChanged;
            shadowQualityDropdown.OnValueChanged += OnShadowsQualityValueChanged;
        }

        private void OnDisable()
        {
            resolutionDropdown.OnValueChanged -= OnResolutionValueChanged;
            fullscreenDropdown.OnValueChanged -= OnFullscreenValueChanged;
            vSyncToggle.OnValueChanged -= OnVSyncToggleValueChanged;
            graphicQualityDropdown.OnValueChanged -= OnQualityLevelValueChanged;
            antiAliasingDropdown.OnValueChanged -= OnAntiAliasingValueChanged;
            anisotropicFilteringDropdown.OnValueChanged -= OnAnisotropicFilteringValueChanged;
            shadowQualityDropdown.OnValueChanged -= OnShadowsQualityValueChanged;
        }

        public void Init()
        {
            var graphicsSettings = settingsModel.GraphicsSettings;

            resolutionDropdown.Init("Resolution", graphicsSettings.ResolutionWidth, graphicsSettings.ResolutionHeight);
            fullscreenDropdown.Init("FullScreenMode", new string[] { "ExclusiveFullScreen", "FullScreenWindow", "MaximizedWindow" }, graphicsSettings.FullScreenMode);
            vSyncToggle.Init("VSync", graphicsSettings.VSync);
            graphicQualityDropdown.Init("GraphicQuality", new string[] { "Low", "Medium", "High", "UltraHigh" }, graphicsSettings.GraphicQuality);
            antiAliasingDropdown.Init("AntiAliasing", new string[] { "Off", "x2", "x4", "x8" }, graphicsSettings.AntiAliasing);
            anisotropicFilteringDropdown.Init("AnisotropicFiltering", new string[] { "Disable", "Enable" }, graphicsSettings.AnisotropicFiltering);
            shadowQualityDropdown.Init("ShadowQuality", new string[] { "Disable", "HardOnly", "All" }, graphicsSettings.ShadowQuality);
        }

        private void OnResolutionValueChanged(int w, int h) => signalBus.Fire(new ApplicationSignals.SetResolution(w, h));
        private void OnFullscreenValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetFullScreenMode(value));
        private void OnVSyncToggleValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetVSync(value));
        private void OnQualityLevelValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetQualityLevel(value));
        private void OnAntiAliasingValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetAntiAliasing(value));
        private void OnAnisotropicFilteringValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetAnisotropicFiltering(value));
        private void OnShadowsQualityValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetShadowsQuality(value));
    }
}