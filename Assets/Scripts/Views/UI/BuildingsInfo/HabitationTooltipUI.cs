using Models.Habitation;
using Models.Helpers;
using TMPro;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class HabitationTooltipUI : BuildingTooltipUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text residentsCountLabel;
        [SerializeField] private HabitationRequirmentsTooltipUI requirmentsPanel;
        [Space]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private GameObject downgradePanel;

        private HabitatModel habitatModel;

        public void Init(Transform buildingTransform, HabitatModel habitatModel)
        {
            this.buildingTransform = buildingTransform;
            this.habitatModel = habitatModel;

            this.habitatModel.OnValueChanged += RefreshUI;
            RefreshUI();

            requirmentsPanel.LoadRequirements(habitatModel);
        }

        private void OnDisable()
        {
            habitatModel.OnValueChanged -= RefreshUI;
        }

        private void RefreshUI()
        {
            nameLabel.text = $"{habitatModel.Name} ({habitatModel.CurrentLevel})";
            residentsCountLabel.text = $"Residents: {habitatModel.Residents.Count}/{habitatModel.MaxResidents}";

            BuildingStateChanging(habitatModel.LevelChangeState);
        }

        private void BuildingStateChanging(LevelChangeState changeState)
        {
            if (changeState == LevelChangeState.Upgrading)
            {
                upgradePanel.SetActive(true);
                downgradePanel.SetActive(false);
            }
            else if (changeState == LevelChangeState.Downgrading)
            {
                upgradePanel.SetActive(false);
                downgradePanel.SetActive(true);
            }
            else
            {
                upgradePanel.SetActive(false);
                downgradePanel.SetActive(false);
            }
        }
    }
}