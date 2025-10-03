using Models.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Buildings
{
    public class DistributionCommodityElementUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text nameLabel;

        public void RefreshUI(CommodityModel commodity)
        {
            //image.sprite = commodity.Sprite;
            nameLabel.text = $"{commodity.Name} {commodity.Quantity}/{commodity.MaxQuantity}";
        }
    }
}