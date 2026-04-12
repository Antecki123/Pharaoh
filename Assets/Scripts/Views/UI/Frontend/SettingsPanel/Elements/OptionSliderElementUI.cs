using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public class OptionSliderElementUI : MonoBehaviour
    {
        public Action<float> OnValueChanged;

        [SerializeField] private TMP_Text optionTypeLabel;
        [SerializeField] private Slider slider;

        private string optionKey;

        public void Init(string optionKey, float currentValue = 1)
        {
            this.optionKey = optionKey;

            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();

            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = Mathf.Clamp01(currentValue);
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            slider.onValueChanged.AddListener(SliderValueChanged);

            UpdateLocalizedText();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            slider.onValueChanged.RemoveAllListeners();
        }

        private void SliderValueChanged(float value)
        {
            OnValueChanged?.Invoke(value);
        }

        private void OnLocaleChanged(Locale locale) => UpdateLocalizedText();

        private void UpdateLocalizedText()
        {
            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();
        }
    }
}