using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.GameInterfaces
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        private void OnEnable()
        {
            Time.timeScale = 0;

            resumeButton.onClick.AddListener(OnResumeButtonClick);
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