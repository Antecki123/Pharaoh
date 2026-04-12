using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Views.Ui.Frontend
{
    public class OptionDropdownElementUI : MonoBehaviour
    {
        public Action<int> OnValueChanged;

        [SerializeField] private TMP_Text optionTypeLabel;
        [SerializeField] private TMP_Dropdown dropdown;

        private string optionKey;
        private string[] values = Array.Empty<string>();

        public void Init(string optionKey, string[] values, int currentIndex = 0)
        {
            this.optionKey = optionKey;
            this.values = values;

            dropdown.ClearOptions();

            foreach (string value in values)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(value));
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
            OnValueChanged?.Invoke(index);
        }

        private void OnLocaleChanged(Locale locale) => UpdateLocalizedText();

        private void UpdateLocalizedText()
        {
            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();

            for (int i = 0; i < dropdown.options.Count; i++)
            {
                var table = LocalizationSettings.StringDatabase.GetTable("Settings");

                if (table == null)
                    continue;

                var entry = table.GetEntry(values[i]);

                if (entry != null)
                {
                    dropdown.options[i].text = entry.GetLocalizedString();
                }
            }

            dropdown.RefreshShownValue();
        }
    }
}