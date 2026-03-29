using Controllers.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public class SplashScreenUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        private FrontendManager frontendManager;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.MainMenuPanel));
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveAllListeners();
        }
    }
}