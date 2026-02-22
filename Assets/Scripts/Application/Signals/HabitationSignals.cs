using Models.Habitation;
using Views.Construction;
using Zenject;

namespace App.Signals
{
    public class HabitationSignals
    {
        public HabitationSignals(DiContainer container)
        {
            container.DeclareSignal<RegisterHabitat>();
            container.DeclareSignal<UnregisterHabitat>();
        }

        public class RegisterHabitat
        {
            public HabitatModel HabitatModel { get; private set; }

            public BuildingView BuildingView { get; private set; }

            public RegisterHabitat(HabitatModel habitatModel, BuildingView buildingView)
            {
                HabitatModel = habitatModel;
                BuildingView = buildingView;
            }
        }

        public class UnregisterHabitat
        {
            public HabitatModel HabitatModel { get; private set; }

            public UnregisterHabitat(HabitatModel habitatModel)
            {
                HabitatModel = habitatModel;
            }
        }
    }
}