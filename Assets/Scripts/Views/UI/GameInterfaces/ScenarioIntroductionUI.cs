using App.Signals;
using Models.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.GameInterfaces
{
    public class ScenarioIntroductionUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text scenarioDescription;
        [Space]
        [SerializeField] private Button beginGameButton;
        [SerializeField] private Image scenarioImage;

        [Inject] private SignalBus signalBus;
        [Inject] private ScenarioModel scenarioModel;

        private void OnEnable()
        {
            var titleLocalizedString = new LocalizedString("ScenarioTitles", scenarioModel.Scenario.ScenarioName);
            titleLabel.text = titleLocalizedString.GetLocalizedString();

            var scenarioDescriptionLocalizedString = new LocalizedString("ScenarioDescription", scenarioModel.Scenario.ScenarioName);
            scenarioDescription.text = scenarioDescriptionLocalizedString.GetLocalizedString();

            beginGameButton.onClick.AddListener(() =>
            {
                signalBus.Fire(new GameControlSignals.ScenarioStarted());
                gameObject.SetActive(false);
            });

            LoadImage();
        }

        private void OnDisable()
        {
            beginGameButton.onClick.RemoveAllListeners();
        }

        private void LoadImage()
        {
            var handle = Addressables.LoadAssetAsync<Sprite>($"IntroductionImagesAtlas[{scenarioModel.Scenario.ScenarioName}]");
            handle.Completed += h =>
            {
                if (h.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    scenarioImage.sprite = h.Result;
                }
            };
        }
    }
}