using App.Signals;
using Models.Application;
using UnityEngine;
using Zenject;

namespace Views.Ui.Frontend
{
    public class GeneralOptionsPanelUI : OptionsPanel
    {
        [SerializeField] private OptionDropdownElementUI languageDropdown;

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
            languageDropdown.OnValueChanged += OnLanguageValueChanged;
        }

        private void OnDisable()
        {
            languageDropdown.OnValueChanged -= OnLanguageValueChanged;
        }

        public void Init()
        {
            var generalSettings = settingsModel.GeneralSettings;
            
            languageDropdown.Init("Language", new string[] { "English", "Polish" }, generalSettings.Language);
        }

        private void OnLanguageValueChanged(int value) => signalBus.Fire(new ApplicationSignals.SetLanguage(value));
    }
}