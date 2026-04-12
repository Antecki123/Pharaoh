using App.Signals;
using Controllers.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.Frontend
{
    public class SplashScreenUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        [Inject] private SignalBus signalBus;

        private FrontendManager frontendManager;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;
        }

        private void OnEnable()
        {
            signalBus.Subscribe<ApplicationSignals.GameInitialized>(OnGameInitialized);
            startButton.onClick.AddListener(OnOpenPanel);
        }

        private void OnDisable()
        {
            signalBus.Unsubscribe<ApplicationSignals.GameInitialized>(OnGameInitialized);
            startButton.onClick.RemoveAllListeners();
        }

        private void OnGameInitialized() => startButton.gameObject.SetActive(true);

        private void OnOpenPanel() => frontendManager.OpenPanel(FrontendPanel.MainMenuPanel);
    }
}