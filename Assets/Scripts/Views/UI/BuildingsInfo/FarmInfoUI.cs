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

        private WorkplaceModel workplaceModel;

        private void OnEnable() => workplaceModel.OnValueChanged += RefreshUI;
        private void OnDisable() => workplaceModel.OnValueChanged -= RefreshUI;

        public void Init(Transform buildingTransform, WorkplaceModel workplace)
        {
            this.buildingTransform = buildingTransform;
            this.workplaceModel = workplace;

            gameObject.SetActive(true);
            RefreshUI();
        }

        private void RefreshUI()
        {
            nameLabel.text = workplaceModel.Name;
            workersLabel.text = $"Current workers: {workplaceModel.Workers.Count}/{workplaceModel.MaxWorkersCount}";

            wheatCommodityElement.Init(workplaceModel.Storage[0].Name, workplaceModel.Storage[0].Quantity, workplaceModel.Storage[0].MaxQuantity);
            linenCommodityElement.Init(workplaceModel.Storage[1].Name, workplaceModel.Storage[1].Quantity, workplaceModel.Storage[1].MaxQuantity);
        }
    }
}