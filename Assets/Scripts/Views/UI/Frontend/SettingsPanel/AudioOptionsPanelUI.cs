using App.Signals;
using Models.Application;
using UnityEngine;
using Zenject;

namespace Views.Ui.Frontend
{
    public class AudioOptionsPanelUI : OptionsPanel
    {
        [SerializeField] private OptionSliderElementUI masterVolumeSlider;
        [SerializeField] private OptionSliderElementUI speechVolumeSlider;
        [SerializeField] private OptionSliderElementUI effectsVolumeSlider;
        [SerializeField] private OptionDropdownElementUI subtitlesDropdown;
        [SerializeField] private OptionDropdownElementUI subtitlesSizeDropdown;

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
            masterVolumeSlider.OnValueChanged += OnMasterVolumeValueChanged;
            speechVolumeSlider.OnValueChanged += OnSpeechVolumeValueChanged;
            effectsVolumeSlider.OnValueChanged += OnEffectsVolumeValueChanged;
            subtitlesDropdown.OnValueChanged += OnSubtitlesValueChanged;
            subtitlesSizeDropdown.OnValueChanged += OnSubtitlesSizeValueChanged;
        }

        private void OnDisable()
        {
            masterVolumeSlider.OnValueChanged -= OnMasterVolumeValueChanged;
            speechVolumeSlider.OnValueChanged -= OnSpeechVolumeValueChanged;
            effectsVolumeSlider.OnValueChanged -= OnEffectsVolumeValueChanged;
            subtitlesDropdown.OnValueChanged -= OnSubtitlesValueChanged;
            subtitlesSizeDropdown.OnValueChanged -= OnSubtitlesSizeValueChanged;
        }

        public void Init()
        {
            var audioSettings = settingsModel.AudioSettings;

            masterVolumeSlider.Init("MasterVolume", audioSettings.MasterVolume);
            speechVolumeSlider.Init("SpeechVolume", audioSettings.SpeechVolume);
            effectsVolumeSlider.Init("EffectsVolume", audioSettings.EffectsVolume);
            subtitlesDropdown.Init("Subtitles", new string[] { "Off", "On" }, audioSettings.Subtitles);
            subtitlesSizeDropdown.Init("SubtitlesSize", new string[] { "Small", "Medium", "Big" }, audioSettings.SubtitlesSize);
        }

        private void OnMasterVolumeValueChanged(float value) => signalBus.Fire(new ApplicationSignals.SetMasterVolume(value));
        private void OnSpeechVolumeValueChanged(float value) => signalBus.Fire(new ApplicationSignals.SetSpeechVolume(value));
        private void OnEffectsVolumeValueChanged(float value) => signalBus.Fire(new ApplicationSignals.SetEffectsVolume(value));
        private void OnSubtitlesValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetSubtitles(value));
        private void OnSubtitlesSizeValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetSubtitlesSize(value));
    }
}