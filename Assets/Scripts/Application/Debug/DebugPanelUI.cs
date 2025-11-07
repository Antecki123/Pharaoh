using App.Configs;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace App.Debug
{
    public class DebugPanelUI : MonoBehaviour
    {
        private event Action<bool> OnPanelActiveChange;

        [SerializeField] private GameObject panel;
        [Space]
        [SerializeField] private TMP_Text fpsCounter;
        [SerializeField] private TMP_Text debugText;
        [Space]
        [SerializeField] private Button navigationButton;
        [SerializeField] private Button settlersButton;
        [Space]
        [SerializeField] private GameObject navigationScrollList;
        [SerializeField] private SettlersDebugListUI settlersScrollList;

        [Inject] private GameConfig gameConfig;
        private DebugLogger debugLogger;

        private DateTime lastRefresh = DateTime.MinValue;
        private TimeSpan refreshSpan = TimeSpan.FromMilliseconds(250);

        private void Awake()
        {
            debugLogger = new DebugLogger(3f);
            debugLogger.OnRefresh += (log) => debugText.text = log;

            OnPanelActiveChange += PanelActiveChange;
        }

        private void Update()
        {
            if (gameConfig.DebugEnabled && Input.GetKeyDown(KeyCode.F1))
            {
                panel.SetActive(!panel.activeSelf);
                OnPanelActiveChange?.Invoke(panel.activeSelf);
            }

            if (DateTime.UtcNow - lastRefresh >= refreshSpan)
            {
                fpsCounter.text = $"FPS: {1 / Time.deltaTime:F0}";
                lastRefresh = DateTime.UtcNow;
            }
        }

        private void PanelActiveChange(bool status)
        {
            if (status)
            {
                Application.logMessageReceived += HandleLog;

                navigationButton.onClick.AddListener(() => ExpandNavigationList(navigationScrollList));
                settlersButton.onClick.AddListener(() => ExpandNavigationList(settlersScrollList.gameObject));
            }
            else
            {
                Application.logMessageReceived -= HandleLog;

                navigationButton.onClick.RemoveAllListeners();
                settlersButton.onClick.RemoveAllListeners();

                navigationScrollList.SetActive(false);
                settlersScrollList.gameObject.SetActive(false);
            }
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            _ = debugLogger.AddMessage(condition, DateTime.Now);
        }

        private void ExpandNavigationList(GameObject openedList)
        {
            var shouldBeActive = !openedList.activeSelf;

            navigationScrollList.SetActive(false);
            settlersScrollList.gameObject.SetActive(false);

            openedList.SetActive(shouldBeActive);
        }
    }

    public class DebugLogger
    {
        public event Action<string> OnRefresh;

        private readonly Queue<string> debugLogs  = new Queue<string>();

        private readonly float durationTime = 1f;

        public DebugLogger(float durationTime = 1f)
        {
            this.durationTime = durationTime;
        }

        public async UniTask AddMessage(string condition, DateTime logTime)
        {
            debugLogs.Enqueue($"[{logTime}] {condition}");
            RefreshLogger();

            await UniTask.WaitForSeconds(durationTime);

            debugLogs.Dequeue();
            RefreshLogger();
        }

        private void RefreshLogger()
        {
            var logOutput = string.Empty;

            foreach (var log in debugLogs)
                logOutput += $"{log}\n";

            OnRefresh?.Invoke(logOutput);
        }
    }
}