using App.Signals;
using Controllers.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.Frontend
{
    public class NewGamePanelUI : MonoBehaviour
    {
        [SerializeField] private Button campaignGameButton;
        [SerializeField] private Button customGameButton;
        [SerializeField] private Button returnButton;

        [Inject] private SignalBus signalBus;

        private FrontendManager frontendManager;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;
        }

        private void OnEnable()
        {
            returnButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.MainMenuPanel));
            campaignGameButton.onClick.AddListener(() =>
            {
                signalBus.Fire(new ApplicationSignals.LoadSceneRequest("Scenario01"));
            });
        }

        private void OnDisable()
        {
            returnButton.onClick.RemoveAllListeners();
            customGameButton.onClick.RemoveAllListeners();
        }
    }
}