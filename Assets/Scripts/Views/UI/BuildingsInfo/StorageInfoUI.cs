using Models.Economy;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class StorageInfoUI : BuildingInfoUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private RectTransform commoditiesContainer;
        [SerializeField] private CommodityElementUI commodityElementPrefab;

        private List<CommodityElementUI> commoditiesToShow = new List<CommodityElementUI>();
        private StorageModel storageModel;

        private void OnEnable() => storageModel.OnValueChanged += RefreshUI;
        private void OnDisable() => storageModel.OnValueChanged -= RefreshUI;

        public void Init(Transform buildingTransform, StorageModel storageModel)
        {
            this.buildingTransform = buildingTransform;
            this.storageModel = storageModel;

            RefreshUI();
            gameObject.SetActive(true);
        }

        private void RefreshUI()
        {
            nameLabel.text = storageModel.Name;

            commoditiesToShow.ForEach(x => Destroy(x.gameObject));
            commoditiesToShow.Clear();
            for (int i = 0; i < storageModel.Storage.Count; i++)
            {
                var commodityToShow = Instantiate(commodityElementPrefab, commoditiesContainer);
                commoditiesToShow.Add(commodityToShow);
            }

            for (int i = 0; i < commoditiesToShow.Count; i++)
            {
                commoditiesToShow[i].RefreshUI(storageModel.Storage[i].Name.ToString(),
                    storageModel.Storage[i].Quantity,
                    storageModel.Storage[i].MaxQuantity);
            }
        }
    }
}