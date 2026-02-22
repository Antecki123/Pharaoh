using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Buildings
{
    public class CommodityElementUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text quantityLabel;
        [SerializeField] private Slider quantitySlider;

        public void RefreshUI(string name, float quantity, float maxQuantity)
        {
            nameLabel.text = name;
            quantityLabel.text = $"{Mathf.FloorToInt(quantity)}/{maxQuantity}";
            quantitySlider.value = quantity / maxQuantity;
        }
    }
}