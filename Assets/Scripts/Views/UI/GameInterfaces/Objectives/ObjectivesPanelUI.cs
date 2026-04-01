using Models.Gameplay;
using TMPro;
using UnityEngine;
using Zenject;

namespace Views.Ui.GameInterfaces
{
    public class ObjectivesPanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text missionTitleLabel;
        [SerializeField] private ObjectiveElementUI objectiveElementPrefab;
        [SerializeField] private Transform objectivesContainer;

        private ObjectivesModel objectivesModel;
        private ScenarioModel scenarioModel;

        [Inject]
        public void Constructor(ObjectivesModel objectivesModel, ScenarioModel scenarioModel)
        {
            this.objectivesModel = objectivesModel;
            this.scenarioModel = scenarioModel;
        }

        private void Start()
        {
            missionTitleLabel.text = scenarioModel.Scenario.ScenarioName;

            foreach (var objective in objectivesModel.Objectives)
            {
                var objectiveElement = Instantiate(objectiveElementPrefab);
                objectiveElement.transform.SetParent(objectivesContainer);
                objectiveElement.Init(objective);
            }
        }
    }
}
