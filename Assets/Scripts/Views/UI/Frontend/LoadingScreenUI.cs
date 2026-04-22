using Controllers.SceneManagment;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.Frontend
{
    public class LoadingScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text discriptionLabel;
        [SerializeField] private Slider loadingProgressSlider;

        [Inject] private SceneHandler sceneHandler;

        private void OnEnable() => sceneHandler.OnAssetLoaded += UpdatePanel;

        private void OnDisable() => sceneHandler.OnAssetLoaded -= UpdatePanel;

        private void Awake()
        {
            discriptionLabel.gameObject.SetActive(false);
            loadingProgressSlider.gameObject.SetActive(false);
        }

        private void UpdatePanel(string discription, float value)
        {
            discriptionLabel.gameObject.SetActive(true);
            loadingProgressSlider.gameObject.SetActive(true);

            discriptionLabel.text = new LocalizedString("MainMenu", discription).GetLocalizedString();;
            loadingProgressSlider.value = value;
        }
    }
}