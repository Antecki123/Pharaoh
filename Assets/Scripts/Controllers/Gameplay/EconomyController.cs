using App.Signals;
using Controllers.Work;
using Models.Economy;
using Models.Habitation;
using System;
using Zenject;

namespace Controllers.Gameplay
{
    public class EconomyController : IInitializable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly EconomyModel economyModel;
        private readonly HabitationModel habitationModel;

        public EconomyController(SignalBus signalBus, EconomyModel economyModel, HabitationModel habitationModel)
        {
            this.signalBus = signalBus;
            this.economyModel = economyModel;
            this.habitationModel = habitationModel;
        }


        public void Initialize()
        {
            signalBus.Subscribe<EnvironmentSignals.DateChanged>(CollectTaxes);
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<EnvironmentSignals.DateChanged>(CollectTaxes);
        }

        private void CollectTaxes()
        {
            var totalTaxes = 0;
            foreach (var habitat in habitationModel.Habitations)
            {
                if (habitat.Key.MunicipalServices[typeof(TaxCollectionService)] == 1f)
                {
                    totalTaxes += habitat.Key.Residents.Count * 4;
                    habitat.Key.ReceiveService(new TaxCollectionService(0f));
                }
            }

            economyModel.AddCurrency(totalTaxes);
        }
    }
}