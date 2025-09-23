using Models.Economy;
using TMPro;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class StorageInfoUI : BuildingInfoUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private CommodityElementUI wheatCommodityElement;

        private StorageModel storageModel;

        private void OnEnable() => storageModel.OnValueChanged += RefreshUI;
        private void OnDisable() => storageModel.OnValueChanged -= RefreshUI;

        public void Init(Transform buildingTransform, StorageModel storageModel)
        {
            this.buildingTransform = buildingTransform;
            this.storageModel = storageModel;

            gameObject.SetActive(true);
            RefreshUI();
        }

        private void RefreshUI()
        {
            nameLabel.text = storageModel.Name;
            wheatCommodityElement.Init(storageModel.Name, storageModel.Storage[0].Quantity, storageModel.Storage[0].MaxQuantity);
        }
    }
}