using Models.Work;
using TMPro;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class FarmInfoUI : BuildingInfoUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text workersLabel;
        [Header("Commodity")]
        [SerializeField] private CommodityElementUI wheatCommodityElement;
        [SerializeField] private CommodityElementUI linenCommodityElement;

        private WorkplaceModel workplace;

        private void OnEnable() => workplace.OnValueChanged += RefreshUI;
        private void OnDisable() => workplace.OnValueChanged -= RefreshUI;

        public void Init(Transform buildingTransform, WorkplaceModel workplace)
        {
            this.buildingTransform = buildingTransform;
            this.workplace = workplace;

            gameObject.SetActive(true);
            RefreshUI();
        }

        private void RefreshUI()
        {
            nameLabel.text = workplace.Name;
            workersLabel.text = $"Current workers: {workplace.Workers.Count}/{workplace.MaxWorkersCount}";

            wheatCommodityElement.RefreshUI(
                workplace.StorageModel.Storage[0].Name.ToString(),
                workplace.StorageModel.Storage[0].Quantity,
                workplace.StorageModel.Storage[0].MaxQuantity);
            linenCommodityElement.RefreshUI(
                workplace.StorageModel.Storage[1].Name.ToString(),
                workplace.StorageModel.Storage[1].Quantity,
                workplace.StorageModel.Storage[1].MaxQuantity);
        }
    }
}