using App.Signals;
using Models.Gameplay;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using Zenject;

namespace Views.Ui.GameInterfaces
{
    public class ObjectivesPanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text missionTitleLabel;
        [SerializeField] private ObjectiveElementUI objectiveElementPrefab;
        [SerializeField] private Transform objectivesContainer;

        private List<ObjectiveElementUI> objectivesElementsList = new List<ObjectiveElementUI>();

        private ObjectivesModel objectivesModel;
        private ScenarioModel scenarioModel;
        private SignalBus signalBus;

        [Inject]
        public void Constructor(SignalBus signalBus, ObjectivesModel objectivesModel, ScenarioModel scenarioModel)
        {
            this.signalBus = signalBus;
            this.objectivesModel = objectivesModel;
            this.scenarioModel = scenarioModel;
        }

        private void OnEnable()
        {
            signalBus.Subscribe<GameControlSignals.MissionCompleted>(UpdatePanel);
        }

        private void OnDisable()
        {
            signalBus.TryUnsubscribe<GameControlSignals.MissionCompleted>(UpdatePanel);
        }

        private void Start()
        {
            UpdatePanel();
        }

        private void UpdatePanel()
        {
            foreach (var objective in objectivesElementsList)
                Destroy(objective.gameObject);

            objectivesElementsList.Clear();

            var titleLocalizedString = new LocalizedString("ScenarioTitles", scenarioModel.Scenario.ScenarioName);
            missionTitleLabel.text = titleLocalizedString.GetLocalizedString();

            foreach (var objective in objectivesModel.Objectives)
            {
                var objectiveElement = Instantiate(objectiveElementPrefab);
                objectiveElement.transform.SetParent(objectivesContainer);
                objectiveElement.Init(objective);

                objectivesElementsList.Add(objectiveElement);
            }
        }
    }
}
