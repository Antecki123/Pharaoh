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

                requirement.OnValueChanged += RefreshUI;
                RefreshUI();
            }
        }

        private void OnDisable()
        {
            foreach (var requirement in habitatModel.HabitationRequirements)
            {
                requirement.OnValueChanged -= RefreshUI;
            }
        }

        private void RefreshUI()
        {
            for (int i = 0; i < commodityElements.Count; i++)
            {
                var requirement = habitatModel.HabitationRequirements[i];
                var isActive = habitatModel.CurrentLevel >= requirement.Level;
                commodityElements[i].gameObject.SetActive(isActive);

                if (isActive)
                    commodityElements[i].RefreshUI(requirement.RequirementDefinition.ToString(), requirement.Value, 100);
            }
        }
    }
}