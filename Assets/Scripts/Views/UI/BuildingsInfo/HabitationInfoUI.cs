using Models.Economy;
using TMPro;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class HabitationInfoUI : BuildingInfoUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text residentsCountLabel;
        [Header("Commodity")]
        [SerializeField] private CommodityElementUI foodCommodityElement;
        [SerializeField] private CommodityElementUI beerCommodityElement;
        [SerializeField] private CommodityElementUI clothesCommodityElement;

        private HabitatModel habitatModel;

        private void OnEnable() => habitatModel.OnValueChanged += RefreshUI;
        private void OnDisable() => habitatModel.OnValueChanged -= RefreshUI;

        public void Init(Transform buildingTransform, HabitatModel habitatModel)
        {
            this.buildingTransform = buildingTransform;
            this.habitatModel = habitatModel;

            gameObject.SetActive(true);
            RefreshUI();
        }

        private void RefreshUI()
        {
            nameLabel.text = habitatModel.Name;
            residentsCountLabel.text = $"Residents: {habitatModel.Residents.Count}/{habitatModel.MaxResidents}";

            foodCommodityElement.Init(habitatModel.Storage[0].Name, habitatModel.Storage[0].Quantity, habitatModel.Storage[0].MaxQuantity);
            beerCommodityElement.Init(habitatModel.Storage[1].Name, habitatModel.Storage[1].Quantity, habitatModel.Storage[1].MaxQuantity);
            clothesCommodityElement.Init(habitatModel.Storage[2].Name, habitatModel.Storage[2].Quantity, habitatModel.Storage[2].MaxQuantity);
        }
    }
}