using Models.Application;
using Zenject;

namespace Views.Ui.Frontend
{
    public class ControlsOptionsPanelUI : OptionsPanel
    {
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

        }

        private void OnDisable()
        {
            
        }

        public void Init()
        {
            var controlsSettings = settingsModel.ControlsSettings;
        }
    }
}