using System.Collections.Generic;
using UnityEngine;
using Views.Ui.Frontend;

namespace Controllers.UI
{
    public class FrontendManager : MonoBehaviour
    {
        [SerializeField] private SplashScreenUI splashScreen;
        [SerializeField] private MainMenuUI mainMenu;
        [SerializeField] private NewGamePanelUI newGamePanel;
        [SerializeField] private SettingsPanelUI settingsPanel;
        [SerializeField] private CreditsPanelUI creditsPanel;
        [SerializeField] private CampainPanelUI campainPanel;

        private Dictionary<FrontendPanel, GameObject> panels = new Dictionary<FrontendPanel, GameObject>();

        private void Awake()
        {
            panels.Add(FrontendPanel.SplashScreenPanel, splashScreen.gameObject);
            panels.Add(FrontendPanel.MainMenuPanel, mainMenu.gameObject);
            panels.Add(FrontendPanel.NewGamePanel, newGamePanel.gameObject);
            panels.Add(FrontendPanel.SettingsPanel, settingsPanel.gameObject);
            panels.Add(FrontendPanel.CreditsPanel, creditsPanel.gameObject);
            panels.Add(FrontendPanel.CampainPanel, campainPanel.gameObject);
        }

        private void Start()
        {
            splashScreen.Init(this);
            mainMenu.Init(this);
            newGamePanel.Init(this);
            settingsPanel.Init(this);
            creditsPanel.Init(this);
            campainPanel.Init(this);
        }

        public void OpenPanel(FrontendPanel panelToOpen)
        {
            foreach (var panel in panels)
                panel.Value.SetActive(panel.Key == panelToOpen);
        }
    }

    public enum FrontendPanel
    {
        SplashScreenPanel,
        MainMenuPanel,
        NewGamePanel,
        SettingsPanel,
        CreditsPanel,
        CampainPanel
    }
}