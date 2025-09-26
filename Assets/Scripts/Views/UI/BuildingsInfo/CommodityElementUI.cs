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

        public void RefreshUI(string name, int quantity, int maxQuantity)
        {
            nameLabel.text = name;
            quantityLabel.text = $"{quantity}/{maxQuantity}";
            quantitySlider.value = (float)quantity / maxQuantity;
        }
    }
}