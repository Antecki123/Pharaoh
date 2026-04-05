using App.Signals;
using UnityEngine;
using Views.Ui.GameInterfaces;
using Zenject;

namespace Controllers.UI
{
    public class GameInterfaceManager : MonoBehaviour
    {
        [SerializeField] private ObjectivesPanelUI objectivesPanel;
        [SerializeField] private ScenarioIntroductionUI scenarioIntroduction;
        [SerializeField] private ScenarioFinishedUI scenarioFinished;
        [SerializeField] private PauseMenuUI pauseMenu;
        [SerializeField] private MissionCompletedUI missionCompleted;

        [Inject] private SignalBus signalBus;


        private void OnEnable()
        {
            signalBus.Subscribe<GameControlSignals.MissionCompleted>(OnMissionCompleted);
            signalBus.Subscribe<GameControlSignals.MissionFailed>(OnMissionFailed);
            signalBus.Subscribe<GameControlSignals.ScenarioFinished>(OnScenarioFinished);
            signalBus.Subscribe<GameControlSignals.OpenPauseMenu>(OnOpenPauseMenu);
        }

        private void OnDisable()
        {
            signalBus.TryUnsubscribe<GameControlSignals.MissionCompleted>(OnMissionCompleted);
            signalBus.TryUnsubscribe<GameControlSignals.MissionFailed>(OnMissionFailed);
            signalBus.TryUnsubscribe<GameControlSignals.ScenarioFinished>(OnScenarioFinished);
            signalBus.TryUnsubscribe<GameControlSignals.OpenPauseMenu>(OnOpenPauseMenu);
        }

        private void Start()
        {
            scenarioIntroduction.gameObject.SetActive(true);
        }

        private void OnMissionCompleted()
        {
            missionCompleted.gameObject.SetActive(true);
        }

        private void OnMissionFailed()
        {

        }

        private void OnScenarioFinished()
        {
            scenarioFinished.gameObject.SetActive(true);
            objectivesPanel.gameObject.SetActive(false);
        }

        private void OnOpenPauseMenu(GameControlSignals.OpenPauseMenu signal)
        {
            pauseMenu.gameObject.SetActive(signal.State);
        }
    }
}