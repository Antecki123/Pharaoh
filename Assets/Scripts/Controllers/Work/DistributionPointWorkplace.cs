using App.Signals;
using Controllers.Construction;
using Models.Construction;
using Models.Economy;
using Models.Work;
using System;
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
            var serviceData = economyImporter.ServiceData[buildingDefinition];
            var definition = new DistributionWorkplaceDefinition()
            {
                Name = buildingDefinition.ToString(),

                RequiredCommodity = economyData.RequiredCommodity != null
                ? new CommodityModel(economyData.RequiredCommodity.Value, economyData.RequiredCommodityQuantity, 0)
                : null,

                Services = CreateService(serviceData),
                ProcessingTime = economyData.ProcessingTime,
                MinimumWorkersCount = economyData.MinimumWorkersCount,
                MaxWorkersCount = economyData.MaxWorkersCount,
                Range = economyData.Range
            };

            return new DistributionPointModel(definition);
        }

        private List<IService> CreateService(List<ServiceData> serviceDataList)
        {
            var result = new List<IService>(serviceDataList.Count);

            foreach (var serviceData in serviceDataList)
            {
                result.Add(serviceData.ServiceType switch
                {
                    ServiceType.TaxCollectionService => new TaxCollectionService(serviceData.Value),
                    ServiceType.ReligionService => new ReligionService(serviceData.Value),
                    ServiceType.HabitationRequirementService => new HabitationRequirementService(
                        serviceData.HabitatRequirementDefinition
                        ?? throw new InvalidOperationException("HabitatRequirementDefinition is required for HabitationRequirementService."),
                        serviceData.Value),
                    ServiceType.FireProtectionService => new FireProtectionService(serviceData.Value),
                    _ => throw new NotSupportedException($"Unsupported service type: {serviceData.ServiceType}")
                });
            }

            return result;
        }

        private void Work(DistributionWorkplacePresenter workplace)
        {
            supplyModel.SupplyTargets.TryGetValue(workplace.View, out storage);
            requiredCommodity = workplace.Model.WorkplaceDefinition.RequiredCommodity;

            if (workplace.Model.CurrentWorkersCount < workplace.Model.WorkplaceDefinition.MinimumWorkersCount)
                return;

            if (requiredCommodity != null && requiredCommodity.Quantity == 0)
            {
                ScheduleTransport(workplace);
                return;
            }

            if (workplace.Model.IsServiceAgentAvailable)
            {
                var progress = workplace.Model.ProcessingProgress;
                var progressDelta = Time.deltaTime / workplace.Model.WorkplaceDefinition.ProcessingTime;
                workplace.Model.SetProcessingProgress(progress + progressDelta);

                if (workplace.Model.ProcessingProgress >= 1)
                {
                    storage?.RemoveCommodity(requiredCommodity);
                    workplace.Model.SetProcessingProgress(0);
                    DistributeResources(workplace);
                }
            }
        }

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

                if (dist >= workplace.Model.WorkplaceDefinition.Range)
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
                Services = workplace.Model.WorkplaceDefinition.Services,
                AvailableTiles = workplace.Model.InfluencedTiles
            };

            void OnAgentReturn() => workplace.Model.ReturnServiceAgent();
            signalBus.Fire(new WorkplaceSignals.SpawnServiceAgent(serviceAgentPayload, OnAgentReturn, workplace.Model));
        }

        private void ScheduleTransport(DistributionWorkplacePresenter workplace)
        {
            if (!workplace.Model.IsCarrierAvailable)
                return;

            var result = BuildCarrierTasks(out Queue<CarrierTask> tasks, workplace);
            if (result == false)
                return;

            workplace.Model.UseCarrier();

            void OnCarrierReturn() => workplace.Model.ReturnCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, OnCarrierReturn, workplace.Model));
        }

        private bool BuildCarrierTasks(out Queue<CarrierTask> tasks, DistributionWorkplacePresenter workplace)
        {
            tasks = default;

            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(
                workplace.View.transform.position, requiredCommodity.Name, requiredCommodity.Quantity);

            if (targetWithFreeSpace == null)
                return false;

            var taskBuilder = new CarrierTaskBuilder()
                .AddTaskWithReservation(storage, targetWithFreeSpace, new CommodityModel()
                {
                    Name = requiredCommodity.Name,
                    Quantity = requiredCommodity.Quantity
                },
                ReservationType.Space)
                .AddTask(targetWithFreeSpace, storage);

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