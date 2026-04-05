using App.Signals;
using Controllers.SceneManagment;
using Controllers.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.Frontend
{
    public class CampainPanelUI : MonoBehaviour
    {
        [SerializeField] private Button chapter1Button;
        [SerializeField] private Button chapter2Button;
        [SerializeField] private Button chapter3Button;
        [Space]
        [SerializeField] private Button returnButton;

        private FrontendManager frontendManager;

        [Inject] private SignalBus signalBus;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;
        }

        private void OnEnable()
        {
            chapter1Button.onClick.AddListener(() => signalBus.Fire(new ApplicationSignals.LoadSceneRequest(SceneName.Chapter01)));
            chapter2Button.onClick.AddListener(() => signalBus.Fire(new ApplicationSignals.LoadSceneRequest(SceneName.Chapter02)));
            chapter3Button.onClick.AddListener(() => signalBus.Fire(new ApplicationSignals.LoadSceneRequest(SceneName.Chapter03)));

            returnButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.NewGamePanel));
        }

        private void OnDisable()
        {
            chapter1Button.onClick.RemoveAllListeners();
            chapter2Button.onClick.RemoveAllListeners();
            chapter3Button.onClick.RemoveAllListeners();

            returnButton.onClick.RemoveAllListeners();
        }
    }
}