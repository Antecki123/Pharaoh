using Controllers.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public abstract class OptionsPanel : MonoBehaviour { }

    public class SettingsPanelUI : MonoBehaviour
    {
        [SerializeField] private Button generalButton;
        [SerializeField] private Button graphicsButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Button controlsButton;
        [Space]
        [SerializeField] private Button returnButton;
        [Space]
        [SerializeField] private GeneralOptionsPanelUI generalOptionsPanel;
        [SerializeField] private GraphicsOptionsPanelUI graphicsOptionsPanel;
        [SerializeField] private AudioOptionsPanelUI audioOptionsPanel;
        [SerializeField] private ControlsOptionsPanelUI controlsOptionsPanel;

        private FrontendManager frontendManager;
        private HashSet<OptionsPanel> tabs;

        public void Init(FrontendManager frontendManager)
        {
            this.frontendManager = frontendManager;

            tabs = new HashSet<OptionsPanel>()
            {
                generalOptionsPanel,
                graphicsOptionsPanel,
                audioOptionsPanel,
                controlsOptionsPanel
            };

            generalOptionsPanel.Init();
            graphicsOptionsPanel.Init();
            audioOptionsPanel.Init();
            controlsOptionsPanel.Init();
        }

        private void OnEnable()
        {
            generalButton.onClick.AddListener(() => OpenTab(generalOptionsPanel));
            graphicsButton.onClick.AddListener(() => OpenTab(graphicsOptionsPanel));
            audioButton.onClick.AddListener(() => OpenTab(audioOptionsPanel));
            controlsButton.onClick.AddListener(() => OpenTab(controlsOptionsPanel));

            returnButton.onClick.AddListener(OnReturnButtonClick);

            OpenTab(generalOptionsPanel);
        }

        private void OnDisable()
        {
            generalButton.onClick.RemoveAllListeners();
            graphicsButton.onClick.RemoveAllListeners();
            audioButton.onClick.RemoveAllListeners();
            controlsButton.onClick.RemoveAllListeners();

            returnButton.onClick.RemoveAllListeners();
        }

        private void OpenTab(OptionsPanel selectedTab)
        {
            foreach (var tab in tabs)
            {
                tab.gameObject.SetActive(tab == selectedTab);
            }
        }

        private void OnReturnButtonClick()
        {
            frontendManager.OpenPanel(FrontendPanel.MainMenuPanel);
        }
    }
}