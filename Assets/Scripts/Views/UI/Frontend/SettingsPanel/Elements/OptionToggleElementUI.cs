using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Views.Ui.Frontend
{
    public class OptionToggleElementUI : MonoBehaviour
    {
        public Action<int> OnValueChanged;

        [SerializeField] private TMP_Text optionTypeLabel;
        [SerializeField] private Toggle toggle;

        private string optionKey;

        public void Init(string optionKey, int currentIndex)
        {
            this.optionKey = optionKey;

            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();
            toggle.isOn = currentIndex > 0;
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            toggle.onValueChanged.AddListener(ToggleStateChanged);

            UpdateLocalizedText();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            toggle.onValueChanged.RemoveAllListeners();
        }

        private void ToggleStateChanged(bool state)
        {
            OnValueChanged?.Invoke(state ? 1 : 0);
        }

        private void OnLocaleChanged(Locale locale) => UpdateLocalizedText();

        private void UpdateLocalizedText()
        {
            optionTypeLabel.text = new LocalizedString("Settings", $"{optionKey}Label").GetLocalizedString();
        }
    }
}