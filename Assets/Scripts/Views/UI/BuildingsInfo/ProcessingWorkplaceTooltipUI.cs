using Models.Work;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Views.Ui.Buildings
{
    public class ProcessingWorkplaceTooltipUI : BuildingTooltipUI
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text workersLabel;
        [Space]
        [SerializeField] private RectTransform commoditiesContainer;
        [SerializeField] private CommodityElementUI commodityElementPrefab;
        [SerializeField] private ProgressPanelElementUI progressPanel;

        private List<CommodityElementUI> commoditiesToShow = new List<CommodityElementUI>();
        private WorkplaceModel workplace;

        public void Init(Transform buildingTransform, WorkplaceModel workplace)
        {
            this.buildingTransform = buildingTransform;
            this.workplace = workplace;

            //this.workplace.OnValueChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDisable()
        {
            //workplace.OnValueChanged -= RefreshUI;
        }

        private void RefreshUI()
        {
            /*nameLabel.text = workplace.Name;
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

            progressPanel.RefreshUI(workplace.ProcessingProgress);*/
        }
    }
}