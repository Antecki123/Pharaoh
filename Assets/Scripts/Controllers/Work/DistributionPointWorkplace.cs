using App.Signals;
using Models.Construction;
using Models.Economy;
using Models.Helpers;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Views.Settler.Workers;
using Zenject;

namespace Controllers.Work
{
    public class DistributionPointWorkplace : IWorkplace, ISupplyTarget
    {
        public DistributionPointModel DistributionModel => distributionModel;
        public HashSet<Vector2Int> InfluencedTiles => influencedTiles;

        private readonly SignalBus signalBus;
        private readonly SupplyModel supplyModel;
        private readonly DistributionPointModel distributionModel;
        private readonly ConstructionGrid constructionGrid;
        private readonly BuildingView buildingView;

        private Timer resourceRefreshTimer;

        private HashSet<Vector2Int> influencedTiles = new HashSet<Vector2Int>();
        private int influenceDistance;

        public DistributionPointWorkplace(SignalBus signalBus, SupplyModel supplyModel, DistributionPointModel distributionModel, ConstructionGrid constructionGrid,
            BuildingView buildingView, int influenceDistance)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.distributionModel = distributionModel;
            this.constructionGrid = constructionGrid;
            this.buildingView = buildingView;
            this.influenceDistance = influenceDistance;

            resourceRefreshTimer = new Timer(5f);

            constructionGrid.OnValueChanged += CalculateInfluenceRange;
            CalculateInfluenceRange();
        }

        public BuildingView GetBuildingView()
        {
            return buildingView;
        }

        public bool TryPickCommodity(ref CommodityModel commodity)
        {
            return false;
        }

        public void DeliverCommodity(CommodityModel commodity)
        {
            distributionModel.StorageModel.AddCommodity(commodity);
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableCommodities()
        {
            return distributionModel.StorageModel.GetAvailableCommodities();
        }

        public IReadOnlyCollection<CommodityModel> GetAvailableSpace()
        {
            return distributionModel.StorageModel.GetAvailableSpace();
        }

        public IReservationable GetReservationable()
        {
            return distributionModel.StorageModel;
        }

        public IEmployer GetEmployer()
        {
            return distributionModel;
        }

        public void DestroyWorkplace()
        {
            signalBus.Fire(new WorkplaceSignals.WorklplaceDestroyed(this));
        }

        public void Work()
        {
            if (distributionModel.Workers.Count < distributionModel.MinimumWorkersCount)
                return;

            resourceRefreshTimer.Tick(Time.deltaTime);

            if (!resourceRefreshTimer.IsFinished)
                return;

            if (distributionModel.DistributedCommodity == null)
            {
                DistributeResources();
                resourceRefreshTimer.Reset();
            }
            else
            {
                if (distributionModel.DistributedCommodity.Quantity > 0)
                {
                    if (distributionModel.ServiceAgentsCount <= 0)
                        return;

                    DistributeResources();
                    distributionModel.StorageModel.RemoveCommodity(new CommodityModel()
                    {
                        Name = distributionModel.DistributedCommodity.Name,
                        Quantity = 1
                    });
                }
                else
                {
                    if (distributionModel.CarriersCount <= 0)
                        return;

                    ScheduleTransport(new CommodityModel()
                    {
                        Name = distributionModel.DistributedCommodity.Name,
                        Quantity = distributionModel.DistributedCommodity.MaxQuantity
                    });
                }
            }
        }

        private void CalculateInfluenceRange()
        {
            influencedTiles.Clear();

            var queue = new Queue<(Vector2Int pos, int distance)>();

            foreach (var tile in constructionGrid.GetAllConnectedRoadTiles(buildingView))
            {
                influencedTiles.Add(tile);
                queue.Enqueue((tile, 0));
            }

            while (queue.Count > 0)
            {
                var (current, dist) = queue.Dequeue();

                if (dist >= influenceDistance)
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

                    if (influencedTiles.Contains(neighbor.Position))
                        continue;

                    influencedTiles.Add(neighbor.Position);
                    queue.Enqueue((neighbor.Position, dist + 1));
                }
            }
        }

        private void DistributeResources()
        {
            if (distributionModel.ServiceAgentsCount == 0)
                return;

            distributionModel.UseServiceAgent();

            var serviceAgentPayload = new ServiceAgentPayload()
            {
                Origin = buildingView,
                HabitationRequirementDefinition = distributionModel.HabitationRequirementDefinition,
                AvailableTiles = influencedTiles
            };

            void OnAgentReturn()
            {
                resourceRefreshTimer.Reset();
                distributionModel.ReturnServiceAgent();
            }

            signalBus.Fire(new WorkplaceSignals.SpawnServiceAgent(serviceAgentPayload, OnAgentReturn, this));
        }

        private void ScheduleTransport(CommodityModel commodity)
        {
            if (distributionModel.CarriersCount == 0)
                return;

            var result = BuildCarrierTasks(commodity, out Queue<CarrierTask> tasks);
            if (result == false)
                return;

            distributionModel.UseCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, () => distributionModel.ReturnCarrier(), this));
        }

        private bool BuildCarrierTasks(CommodityModel commodity, out Queue<CarrierTask> tasks)
        {
            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(buildingView.transform.position, commodity.Name);

            if (targetWithCommodity == null)
            {
                tasks = default;
                return false;
            }

            var taskBuilder = new CarrierTaskBuilder();
            taskBuilder
                .AddTask(this, targetWithCommodity)
                .AddTaskWithReservation(targetWithCommodity, this, commodity, ReservationType.Commodity);

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }
}