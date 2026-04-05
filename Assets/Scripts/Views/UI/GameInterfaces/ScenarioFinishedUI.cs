using App.Signals;
using Controllers.SceneManagment;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.GameInterfaces
{
    public class ScenarioFinishedUI : MonoBehaviour
    {
        [SerializeField] private Button continueGameButton;
        [SerializeField] private Button quitButton;

        [Inject] private SignalBus signalBus;

        private void OnEnable()
        {
            continueGameButton.onClick.AddListener(OnContinueGame);
            quitButton.onClick.AddListener(() => signalBus.Fire(new ApplicationSignals.LoadSceneRequest(SceneName.MainMenu)));
        }

        private void OnDisable()
        {
            continueGameButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }

        private void OnContinueGame()
        {
            signalBus.Fire(new GameControlSignals.GameSpeed(1f));
            gameObject.SetActive(false);
        }
    }
}