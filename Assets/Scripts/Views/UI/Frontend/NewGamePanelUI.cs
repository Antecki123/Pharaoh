using Controllers.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public class NewGamePanelUI : MonoBehaviour
    {
        [SerializeField] private Button campaignGameButton;
        [SerializeField] private Button customGameButton;
        [SerializeField] private Button returnButton;

        private FrontendManager frontendManager;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;
        }

        private void OnEnable()
        {
            campaignGameButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.CampainPanel));
            //customGameButton.onClick.AddListener();
            returnButton.onClick.AddListener(() => frontendManager.OpenPanel(FrontendPanel.MainMenuPanel));
        }

        private void OnDisable()
        {
            campaignGameButton.onClick.RemoveAllListeners();
            customGameButton.onClick.RemoveAllListeners();
            returnButton.onClick.RemoveAllListeners();
        }
    }
}