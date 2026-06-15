using App.Signals;
using Controllers.Construction;
using Models.Construction;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class DistributionPointWorkplace
    {
        public class Factory : PlaceholderFactory<DistributionPointWorkplace> { }

        private readonly SignalBus signalBus;
        private readonly SupplyModel supplyModel;
        private readonly ConstructionGrid constructionGrid;
        private readonly WorkplaceEconomyImporter economyImporter;

        private readonly List<DistributionWorkplacePresenter> workplaces = new();

        private StorageModel storage;
        private CommodityModel requiredCommodity;

        public DistributionPointWorkplace(SignalBus signalBus, SupplyModel supplyModel,
            ConstructionGrid constructionGrid, WorkplaceEconomyImporter economyImporter)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.constructionGrid = constructionGrid;
            this.economyImporter = economyImporter;
        }

        public IWorkplace RegisterWorkplace(BuildingView buildingView)
        {
            var workplaceModel = CreateModel(buildingView.BuildingDefinition);
            var workplace = new DistributionWorkplacePresenter(workplaceModel, buildingView);
            workplaces.Add(workplace);

            constructionGrid.OnValueChanged += () => CalculateInfluenceRange(workplace);
            CalculateInfluenceRange(workplace);

            /*workplace.Model.MunicipalServices = new()
            {
                { typeof(FireProtectionService), 1f }
            };*/

            return workplace.Model;
        }

        public void UnregisterWorkplace(BuildingView buildingView)
        {
            foreach (var workplace in workplaces)
            {
                if (workplace.View == buildingView)
                {
                    constructionGrid.OnValueChanged -= () => CalculateInfluenceRange(workplace);
                    workplaces.Remove(workplace);
                }
            }
        }

        public void Tick()
        {
            foreach (var workplace in workplaces)
                Work(workplace);
        }

        private DistributionPointModel CreateModel(BuildingDefinition buildingDefinition)
        {
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var definition = new DistributionWorkplaceDefinition()
            {
                Name = buildingDefinition.ToString(),

                RequiredCommodity = economyData.RequiredCommodity != null
                ? new CommodityModel(economyData.RequiredCommodity.Value, economyData.RequiredCommodityQuantity, 0)
                : null,

                ProcessingTime = economyData.ProcessingTime,
                MinimumWorkersCount = economyData.MinimumWorkersCount,
                MaxWorkersCount = economyData.MaxWorkersCount
            };

            return new DistributionPointModel(definition);
        }

        private void Work(DistributionWorkplacePresenter workplace)
        {
            storage = supplyModel.SupplyTargets[workplace.View];
            requiredCommodity = workplace.Model.WorkplaceDefinition.RequiredCommodity;

            if (workplace.Model.CurrentWorkersCount < workplace.Model.WorkplaceDefinition.MinimumWorkersCount)
                return;

            if (workplace.Model.WorkplaceDefinition.RequiredCommodity == null)
            {
                DistributeResources(workplace);
            }
            else
            {
                if (workplace.Model.WorkplaceDefinition.RequiredCommodity.Quantity > 0)
                {
                    if (!workplace.Model.IsServiceAgentAvailable)
                        return;

                    DistributeResources(workplace);
                    storage.RemoveCommodity(new CommodityModel()
                    {
                        Name = workplace.Model.WorkplaceDefinition.RequiredCommodity.Name,
                        Quantity = 1
                    });
                }
                else
                {
                    if (!workplace.Model.IsCarrierAvailable)
                        return;

                    ScheduleTransport(new CommodityModel()
                    {
                        Name = workplace.Model.WorkplaceDefinition.RequiredCommodity.Name,
                        Quantity = workplace.Model.WorkplaceDefinition.RequiredCommodity.Quantity
                    },
                    workplace);
                }
            }
        }

        /*public void ReceiveService(IService service)
        {
            switch (service)
            {
                case FireProtectionService fireProtection:
                    municipalServices[fireProtection.GetType()] = fireProtection.Value;
                    break;
            }
        }*/

        private void CalculateInfluenceRange(DistributionWorkplacePresenter workplace)
        {
            workplace.Model.InfluencedTiles.Clear();

            var queue = new Queue<(Vector2Int pos, int distance)>();

            foreach (var tile in constructionGrid.GetAllConnectedRoadTiles(workplace.View))
            {
                workplace.Model.InfluencedTiles.Add(tile);
                queue.Enqueue((tile, 0));
            }

            while (queue.Count > 0)
            {
                var (current, dist) = queue.Dequeue();

                if (dist >= workplace.Model.InfluenceData.InfluenceRange)
                    continue;

                Vector2Int[] neighbours =
                {
                    current + Vector2Int.up,
                    current + Vector2Int.down,
                    current + Vector2Int.left,
                    current + Vector2Int.right
                };

                foreach (var neighborPos in neighbours)
                {
                    var neighbor = constructionGrid.GetTileByPosition(neighborPos);

                    if (neighbor == null || neighbor.TileType != TileType.Road)
                        continue;

                    if (workplace.Model.InfluencedTiles.Contains(neighbor.Position))
                        continue;

                    workplace.Model.InfluencedTiles.Add(neighbor.Position);
                    queue.Enqueue((neighbor.Position, dist + 1));
                }
            }
        }

        private void DistributeResources(DistributionWorkplacePresenter workplace)
        {
            if (!workplace.Model.IsServiceAgentAvailable)
                return;

            workplace.Model.UseServiceAgent();

            var serviceAgentPayload = new ServiceAgentPayload()
            {
                Origin = workplace.View,
                Service = workplace.Model.Service,
                AvailableTiles = workplace.Model.InfluencedTiles
            };

            void OnAgentReturn() => workplace.Model.ReturnServiceAgent();

            signalBus.Fire(new WorkplaceSignals.SpawnServiceAgent(serviceAgentPayload, OnAgentReturn, null));
        }

        private void ScheduleTransport(CommodityModel commodity, DistributionWorkplacePresenter workplace)
        {
            if (!workplace.Model.IsCarrierAvailable)
                return;

            var result = BuildCarrierTasks(commodity, out Queue<CarrierTask> tasks, workplace);
            if (result == false)
                return;

            workplace.Model.UseCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, () => workplace.Model.ReturnCarrier(), workplace.Model));
        }

        private bool BuildCarrierTasks(CommodityModel commodity, out Queue<CarrierTask> tasks, DistributionWorkplacePresenter workplace)
        {
            var targetWithCommodity = supplyModel
                .GetClosestStorageWithCommodity(workplace.View.transform.position, commodity.Name, commodity.Quantity);

            if (targetWithCommodity == null)
            {
                tasks = default;
                return false;
            }

            var taskBuilder = new CarrierTaskBuilder();
            taskBuilder
                .AddTask(null, targetWithCommodity)
                .AddTaskWithReservation(targetWithCommodity, null, commodity, ReservationType.Commodity);

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }

    public struct DistributionWorkplacePresenter
    {
        public DistributionPointModel Model { get; private set; }

        public BuildingView View { get; private set; }

        public DistributionWorkplacePresenter(DistributionPointModel model, BuildingView view)
        {
            Model = model;
            View = view;
        }
    }
}