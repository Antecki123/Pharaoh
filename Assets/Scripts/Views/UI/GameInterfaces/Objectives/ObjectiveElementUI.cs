using Models.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.GameInterfaces
{
    public class ObjectiveElementUI : MonoBehaviour
    {
        [SerializeField] private Toggle fulfilledToggle;
        [SerializeField] private TMP_Text objectiveLabel;

        private IObjective objective;

        public void Init(IObjective objective)
        {
            this.objective = objective;
        }

        private void Update()
        {
            fulfilledToggle.isOn = objective.IsFulfilled;
            objectiveLabel.text = $"{objective.Name} [{objective.ProgressDisplay}]";
        }
    }
}