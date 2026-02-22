using Models.Economy;
using Models.Habitation;
using TMPro;
using UnityEngine;
using Zenject;

//https://coolors.co/palette/cb997e-eddcd2-fff1e6-f0efeb-ddbea9-a5a58d-b7b7a4

namespace Views.Ui.GameInterfaces
{
    public class CityUpperInfoPanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text cityName;
        [SerializeField] private TMP_Text currencyCounter;
        [SerializeField] private TMP_Text settlersCounter;

        [Inject] private EconomyModel economyModel;
        [Inject] private HabitationModel habitationModel;

        private void Start()
        {
            cityName.text = "Men-nefer";
        }

        private void Update()
        {
            currencyCounter.text = economyModel.Currency.ToString();
            settlersCounter.text = $"{economyModel.Settlers}/{habitationModel.GetHousingCapacity()}";
        }
    }
}