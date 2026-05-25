using Models.Habitation;
using System.Collections.Generic;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class HabitationRequirmentsTooltipUI : MonoBehaviour
    {
        [SerializeField] private CommodityElementUI commodityElementPrefab;
        [SerializeField] private RectTransform requirementsContainer;

        private HabitatModel habitatModel;
        private List<CommodityElementUI> commodityElements = new List<CommodityElementUI>();

        public void LoadRequirements(HabitatModel habitatModel)
        {
            this.habitatModel = habitatModel;

            foreach (var requirement in habitatModel.HabitationRequirements)
            {
                var commodityElement = Instantiate(commodityElementPrefab);
                commodityElement.transform.SetParent(requirementsContainer);
                commodityElements.Add(commodityElement);

                requirement.Value.OnValueChanged += RefreshUI;
                RefreshUI();
            }
        }

        private void OnDisable()
        {
            foreach (var requirement in habitatModel.HabitationRequirements)
            {
                requirement.Value.OnValueChanged -= RefreshUI;
            }
        }

        private void RefreshUI()
        {
            // TODO: fix later
            /*for (int i = 0; i < commodityElements.Count; i++)
            {
                var requirement = habitatModel.HabitationRequirements[i]; // tutaj nie mo¿na wyci¹gn¹æ indexu z dictionary
                var isActive = habitatModel.CurrentLevel >= requirement.RequiredLevel;
                commodityElements[i].gameObject.SetActive(isActive);

                if (isActive)
                    commodityElements[i].RefreshUI(
                        requirement.RequirementDefinition.ToString(),
                        requirement.CurrentValue,
                        requirement.MaxValue
                        );
            }*/
        }
    }
}