using Models.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Views.Ui.GameInterfaces
{
    public class ObjectiveElementUI : MonoBehaviour
    {
        [SerializeField] private Toggle fulfilledToggle;
        [SerializeField] private TMP_Text objectiveLabel;

        private IObjective objective;
        private LocalizedString objectiveLocalizedString;

        public void Init(IObjective objective)
        {
            this.objective = objective;

            objectiveLocalizedString = new LocalizedString("Gameplay", objective.Name);
        }

        private void Update()
        {
            fulfilledToggle.isOn = objective.IsFulfilled;
            objectiveLabel.text = $"{objectiveLocalizedString.GetLocalizedString()} [{objective.ProgressDisplay}]";
        }
    }
}