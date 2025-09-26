using Models.Work;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Buildings
{
    public class ProcessingWorkplaceInfoUI : BuildingInfoUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text workersLabel;
        [SerializeField] private RectTransform commoditiesContainer;
        [SerializeField] private CommodityElementUI commodityElementPrefab;
        [Space]
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Slider progressSlider;

        private List<CommodityElementUI> commoditiesToShow = new List<CommodityElementUI>();
        private WorkplaceModel workplace;

        private void OnEnable() => workplace.OnValueChanged += RefreshUI;
        private void OnDisable() => workplace.OnValueChanged -= RefreshUI;

        public void Init(Transform buildingTransform, WorkplaceModel workplace)
        {
            this.buildingTransform = buildingTransform;
            this.workplace = workplace;

            RefreshUI();
            gameObject.SetActive(true);
        }

        private void RefreshUI()
        {
            nameLabel.text = workplace.Name;
            workersLabel.text = $"Current workers: {workplace.Workers.Count}/{workplace.MaxWorkersCount}";

            commoditiesToShow.ForEach(x => Destroy(x.gameObject));
            commoditiesToShow.Clear();
            for (int i = 0; i < workplace.StorageModel.Storage.Count; i++)
            {
                var commodityToShow = Instantiate(commodityElementPrefab, commoditiesContainer);
                commoditiesToShow.Add(commodityToShow);
            }

            for (int i = 0; i < commoditiesToShow.Count; i++)
            {
                commoditiesToShow[i].RefreshUI(
                    workplace.StorageModel.Storage[i].Name.ToString(),
                    workplace.StorageModel.Storage[i].Quantity,
                    workplace.StorageModel.Storage[i].MaxQuantity);
            }

            progressLabel.text = $"Progress: {Mathf.RoundToInt(workplace.ProcessingProgress * 100)}%";
            progressSlider.value = workplace.ProcessingProgress;
        }
    }
}