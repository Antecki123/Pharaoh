using App.Signals;
using Models.Economy;
using Models.Environment;
using Models.Habitation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

//https://coolors.co/palette/cb997e-eddcd2-fff1e6-f0efeb-ddbea9-a5a58d-b7b7a4

namespace Views.Ui.GameInterfaces
{
    public class CityUpperInfoPanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text cityName;
        [SerializeField] private TMP_Text date;
        [SerializeField] private TMP_Text currencyCounter;
        [SerializeField] private TMP_Text settlersCounter;
        [Space]
        [SerializeField] private Button pauseMenuButton;

        private EconomyModel economyModel;
        private HabitationModel habitationModel;
        private DateModel dateModel;

        [Inject]
        public void Constructor(SignalBus signalBus, EconomyModel economyModel, HabitationModel habitationModel, DateModel dateModel)
        {
            this.economyModel = economyModel;
            this.habitationModel = habitationModel;
            this.dateModel = dateModel;

            pauseMenuButton.onClick.AddListener(() => signalBus.Fire(new GameControlSignals.OpenPauseMenu(true)));
        }

        private void Start()
        {
            cityName.text = "Men-nefer";
        }

        private void Update()
        {
            date.text = $"{(MonthName)dateModel.CurrentMonth} {CurrentYearForDisplay()}";

            currencyCounter.text = economyModel.Currency.ToString();
            settlersCounter.text = $"{economyModel.Settlers}/{habitationModel.GetHousingCapacity()}";
        }

        private string CurrentYearForDisplay()
        {
            if (dateModel.CurrentYear < 0)
                return $"{Mathf.Abs(dateModel.CurrentYear)} BC";
            else
                return dateModel.CurrentYear.ToString();
        }
    }
}