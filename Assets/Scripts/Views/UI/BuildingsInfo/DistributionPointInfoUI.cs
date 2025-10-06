using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class DistributionPointInfoUI : BuildingInfoUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text workersLabel;
        [SerializeField] private RectTransform commoditiesContainer;
        [SerializeField] private DistributionCommodityElementUI commodityElementPrefab;

        private List<GameObject> commoditiesToShow = new List<GameObject>();
        private DistributionPointModel distributionModel;

        private void OnEnable() => distributionModel.OnValueChanged += RefreshUI;
        private void OnDisable() => distributionModel.OnValueChanged -= RefreshUI;

        public void Init(Transform buildingTransform, DistributionPointModel distributionModel)
        {
            this.buildingTransform = buildingTransform;
            this.distributionModel = distributionModel;

            RefreshUI();
            gameObject.SetActive(true);
        }

        private void RefreshUI()
        {
            nameLabel.text = distributionModel.Name;
            workersLabel.text = $"Current workers: {distributionModel.Workers.Count}/{distributionModel.MaxWorkersCount}";

            commoditiesToShow.ForEach(x => Destroy(x));
            commoditiesToShow.Clear();

            for (int i = 0; i < distributionModel.MarketStalls.Count; i++)
            {
                if (!distributionModel.MarketStalls[i].IsAvailable)
                    continue;

                var commodityToShow = Instantiate(commodityElementPrefab, commoditiesContainer);
                commodityToShow.RefreshUI(new CommodityModel()
                {
                    Name = distributionModel.MarketStalls[i].Commodity.Name,
                    Quantity = distributionModel.MarketStalls[i].Commodity.Quantity * 40,
                    MaxQuantity = distributionModel.MarketStalls[i].Commodity.MaxQuantity * 40
                });

                commoditiesToShow.Add(commodityToShow.gameObject);
            }
        }
    }
}