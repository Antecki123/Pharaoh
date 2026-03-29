using Controllers.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [Space]
        [SerializeField] private TMP_Text versionLabel;

        private FrontendManager frontendManager;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;
        }

        private void OnEnable()
        {
            newGameButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.NewGamePanel));
            settingsButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.SettingsPanel));
            creditsButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.CreditsPanel));
            quitButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            versionLabel.text = $"ver. {Application.version}";
        }

        private void OnDisable()
        {
            newGameButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            creditsButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }
    }
}