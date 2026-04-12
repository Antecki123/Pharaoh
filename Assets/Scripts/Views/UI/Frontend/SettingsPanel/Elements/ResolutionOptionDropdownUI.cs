using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Views.Ui.Frontend
{
    public class ResolutionOptionDropdownUI : MonoBehaviour
    {
        public Action<int, int> OnValueChanged;

        [SerializeField] private TMP_Text optionTypeLabel;
        [SerializeField] private TMP_Dropdown dropdown;

        private string optionKey;
        private Dictionary<string, (int, int)> resolutionsMap = new Dictionary<string, (int, int)>();

        public void Init(string optionKey, int width = 0, int height = 0)
        {
            this.optionKey = optionKey;

            dropdown.ClearOptions();
            resolutionsMap.Clear();

            int currentIndex = 0;
            int i = 0;

            foreach (var value in Screen.resolutions)
            {
                var data = $"{value.width}×{value.height}";

                dropdown.options.Add(new TMP_Dropdown.OptionData(data));
                resolutionsMap[data] = (value.width, value.height);

                if (value.width == width && value.height == height)
                {
                    currentIndex = i;
                }

                i++;
            }

            dropdown.SetValueWithoutNotify(currentIndex);
            dropdown.RefreshShownValue();
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            dropdown.onValueChanged.AddListener(DropdownValueSelected);

            UpdateLocalizedText();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            dropdown.onValueChanged.RemoveAllListeners();
        }

        private void DropdownValueSelected(int index)
        {
            var resolution = dropdown.options[dropdown.value].text;
            OnValueChanged?.Invoke(resolutionsMap[resolution].Item1, resolutionsMap[resolution].Item2);
        }

        private void OnLocaleChanged(Locale locale) => UpdateLocalizedText();

        private void UpdateLocalizedText()
        {
            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();
        }
    }
}