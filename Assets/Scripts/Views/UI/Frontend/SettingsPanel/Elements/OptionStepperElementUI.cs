using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public class OptionStepperElementUI : MonoBehaviour
    {
        public Action<int> OnValueChanged;

        [SerializeField] private TMP_Text optionTypeLabel;
        [Space]
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private Button buttonDecrease;
        [SerializeField] private Button buttonIncrease;

        private string optionKey;
        private int currentIndex;
        private string[] values = Array.Empty<string>();
        private string[] localizedValues = Array.Empty<string>();

        public void Init(string optionKey, string[] values, int currentIndex = 0)
        {
            this.values = values;
            this.optionKey = optionKey;
            this.currentIndex = currentIndex;

            localizedValues = new string[values.Length];

            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();
            valueLabel.text = values[0];
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            buttonDecrease.onClick.AddListener(DecreaseValue);
            buttonIncrease.onClick.AddListener(IncreaseValue);

            UpdateLocalizedText();
            UpdateUI();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

            buttonDecrease.onClick.RemoveAllListeners();
            buttonIncrease.onClick.RemoveAllListeners();
        }

        private void DecreaseValue()
        {
            if (currentIndex > 0)
                currentIndex--;

            OnValueChanged?.Invoke(currentIndex);
            UpdateUI();
        }

        private void IncreaseValue()
        {
            if (currentIndex < values.Length)
                currentIndex++;

            OnValueChanged?.Invoke(currentIndex);
            UpdateUI();
        }

        private void UpdateUI()
        {
            valueLabel.text = localizedValues[currentIndex];

            buttonDecrease.interactable = currentIndex > 0;
            buttonIncrease.interactable = currentIndex < values.Length - 1;
        }

        private void OnLocaleChanged(Locale locale) => UpdateLocalizedText();

        private void UpdateLocalizedText()
        {
            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();

            for (int i = 0; i < values.Length; i++)
            {
                var table = LocalizationSettings.StringDatabase.GetTable("Settings");

                if (table == null)
                    continue;

                var entry = table.GetEntry(values[i]);

                if (entry != null)
                {
                    localizedValues[i] = entry.GetLocalizedString();
                }
                else
                {
                    localizedValues[i] = values[i];
                }
            }
        }
    }
}