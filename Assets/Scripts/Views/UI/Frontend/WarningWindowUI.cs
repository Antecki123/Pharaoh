using Models.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.Frontend
{
    public class WarningWindowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text contentLabel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [Space]
        [SerializeField] private TMP_Text timerLabel;

        private Timer timer;

        [Inject]
        public void Constructor([Inject(Id = "MainCanvas")]Canvas mainCanvas)
        {
            transform.SetParent(mainCanvas.transform);

            var rt = transform as RectTransform;

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public void Init(string descriptionId, Action onConfirm = null, Action onCancel = null)
        {
            var localizedDescription = new LocalizedString("Warnings", descriptionId);
            contentLabel.text = localizedDescription.GetLocalizedString();

            confirmButton.onClick.AddListener(onConfirm.Invoke);
            confirmButton.gameObject.SetActive(onConfirm != null);

            cancelButton.onClick.AddListener(onCancel.Invoke);
            confirmButton.gameObject.SetActive(onCancel != null);
        }

        public WarningWindowUI OnConfirm(Action onConfirm)
        {
            confirmButton.onClick.AddListener(onConfirm.Invoke);
            confirmButton.gameObject.SetActive(true);

            return this;
        }

        public WarningWindowUI OnCancel(Action onCancel)
        {
            cancelButton.onClick.AddListener(onCancel.Invoke);
            confirmButton.gameObject.SetActive(onCancel != null);

            return this;
        }

        public WarningWindowUI AddTimer(float time)
        {
            timer = new Timer(time);

            return this;
        }
    }
}