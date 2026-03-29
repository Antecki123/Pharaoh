using Controllers.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public class SettingsPanelUI : MonoBehaviour
    {
        [SerializeField] private Button returnButton;

        private FrontendManager frontendManager;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;
        }

        private void OnEnable()
        {
            returnButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.MainMenuPanel));
        }

        private void OnDisable()
        {
            returnButton.onClick.RemoveAllListeners();
        }
    }
}