using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views.Ui.Buildings
{
    public class ProgressPanelElementUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Slider progressSlider;

        public void RefreshUI(float value)
        {
            progressLabel.text = $"Progress: {Mathf.RoundToInt(value * 100)}%";
            progressSlider.value = value;
        }
    }
}