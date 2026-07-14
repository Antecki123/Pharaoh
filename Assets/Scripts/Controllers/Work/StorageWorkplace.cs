using Controllers.Construction;
using Models.Economy;
using System.Collections.Generic;
using Views.Construction;
using Zenject;

namespace Controllers.Work
{
    public class StorageWorkplace
    {
        public class Factory : PlaceholderFactory<StorageWorkplace> { }

        private readonly WorkplaceEconomyImporter economyImporter;

        private readonly List<StorageWorkplacePresenter> workplaces = new();

        public StorageWorkplace(WorkplaceEconomyImporter economyImporter)
        {
            this.economyImporter = economyImporter;
        }

        public IWorkplace RegisterWorkplace(BuildingView buildingView)
        {
            var workplaceModel = CreateModel(buildingView.BuildingDefinition);
            var workplace = new StorageWorkplacePresenter(workplaceModel, buildingView);
            workplaces.Add(workplace);

            return workplace.Model;
        }

        public void UnregisterWorkplace(BuildingView buildingView)
        {
            var workplace = workplaces.Find(x => x.View == buildingView);
            workplaces.Remove(workplace);
        }

        private StorageWorkplaceModel CreateModel(BuildingDefinition buildingDefinition)
        {
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var definition = new StorageWorkplaceDefinition()
            {
                Name = buildingDefinition.ToString(),
                MinimumWorkersCount = economyData.MinimumWorkersCount,
                MaxWorkersCount = economyData.MaxWorkersCount
            };

            return new StorageWorkplaceModel(definition);
        }
    }

    public struct StorageWorkplacePresenter
    {
        public StorageWorkplaceModel Model { get; private set; }

        public BuildingView View { get; private set; }

        public StorageWorkplacePresenter(StorageWorkplaceModel model, BuildingView view)
        {
            Model = model;
            View = view;
        }
    }
}