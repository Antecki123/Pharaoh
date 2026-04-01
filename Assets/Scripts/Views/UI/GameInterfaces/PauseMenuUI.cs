using App.Signals;
using Controllers.SceneManagment;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.GameInterfaces
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        [Inject] private SignalBus signalBus;

        private void OnEnable()
        {
            Time.timeScale = 0;

            resumeButton.onClick.AddListener(OnResumeButtonClick);
            quitButton.onClick.AddListener(() => signalBus.Fire(new ApplicationSignals.LoadSceneRequest(SceneName.MainMenu)));
        }

        private void OnDisable()
        {
            resumeButton.onClick.RemoveAllListeners();
            optionsButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }

        private void OnResumeButtonClick()
        {
            Time.timeScale = 1;
            gameObject.SetActive(false);
        }
    }
}