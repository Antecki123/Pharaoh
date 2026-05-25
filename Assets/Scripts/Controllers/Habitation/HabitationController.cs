using App.Signals;
using Models.Habitation;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Controllers.Habitation
{
    public class HabitationController : IInitializable, ITickable
    {
        private readonly SignalBus signalBus;
        private readonly HabitationModel habitation;

        public HabitationController(SignalBus signalBus, HabitationModel habitation)
        {
            this.signalBus = signalBus;
            this.habitation = habitation;
        }

        public void Initialize()
        {
            signalBus.Subscribe<HabitationSignals.RegisterHabitat>(RegisterHabitat);
            signalBus.Subscribe<HabitationSignals.UnregisterHabitat>(UnregisterHabitat);
        }

        public void Tick()
        {
            foreach (var habitat in habitation.Habitations)
            {
                var habitatModel = habitat.Key;
                foreach (var requirement in habitatModel.HabitationRequirements)
                    requirement.Value.Decay(habitatModel.Residents.Count, Time.deltaTime);

                if (habitatModel.LevelChangeState == Models.Helpers.LevelChangeState.None)
                {
                    UpgradeCheck(habitatModel);
                    DowngradeCheck(habitatModel);
                }
                else
                {
                    habitatModel.LevelChange(Time.deltaTime);
                }
            }
        }

        private void RegisterHabitat(HabitationSignals.RegisterHabitat signal)
        {
            habitation.AddHabitation(signal.HabitatModel, signal.BuildingView);
        }

        private void UnregisterHabitat(HabitationSignals.UnregisterHabitat signal)
        {
            habitation.RemoveHabitation(signal.HabitatModel);
        }

        private void UpgradeCheck(HabitatModel habitatModel)
        {
            if (habitatModel.CurrentLevel == habitatModel.MaxLevel)
                return;

            var requirementsForLevel = habitatModel.HabitationRequirements
                .Where(r => r.Value.RequiredLevel == habitatModel.CurrentLevel)
                .ToList();

            var allRequirementsMet = requirementsForLevel
                .Any() && requirementsForLevel
                .All(r => r.Value.CurrentValue > 75);

            if (habitatModel.Residents.Count == habitatModel.MaxResidents && allRequirementsMet)
                habitatModel.SetUpgradeTimer(Time.deltaTime);
            else
                habitatModel.SetUpgradeTimer(Time.deltaTime, true);
        }

        private void DowngradeCheck(HabitatModel habitatModel)
        {
            if (habitatModel.CurrentLevel == habitatModel.MinLevel)
                return;

            var requirementsForLevel = habitatModel.HabitationRequirements
                .Where(r => r.Value.RequiredLevel >= habitatModel.CurrentLevel)
                .ToList();

            var allRequirementsFailed = requirementsForLevel
                .All(r => r.Value.CurrentValue <= 0);

            if (allRequirementsFailed)
                habitatModel.SetDowngradeTimer(Time.deltaTime);
            else
                habitatModel.SetDowngradeTimer(Time.deltaTime, true);
        }
    }
}