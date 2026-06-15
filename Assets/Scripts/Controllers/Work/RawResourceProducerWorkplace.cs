using App.Signals;
using Controllers.Construction;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Views.Construction;
using Zenject;

namespace Controllers.Work
{
    public class RawResourceProducerWorkplace
    {
        public class Factory : PlaceholderFactory<RawResourceProducerWorkplace> { }

        private readonly SignalBus signalBus;
        private readonly SupplyModel supplyModel;
        private readonly WorkplaceEconomyImporter economyImporter;

        private readonly List<WorkplacePresenter> workplaces = new();

        private StorageModel storage;
        private CommodityModel processedCommodity;

        public RawResourceProducerWorkplace(SignalBus signalBus, SupplyModel supplyModel,
            WorkplaceEconomyImporter economyImporter)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.economyImporter = economyImporter;
        }

        public IWorkplace RegisterWorkplace(BuildingView buildingView)
        {
            var workplaceModel = CreateModel(buildingView.BuildingDefinition);
            var workplace = new WorkplacePresenter(workplaceModel, buildingView);
            workplaces.Add(workplace);

            return workplace.Model;
        }

        public void UnregisterWorkplace(BuildingView buildingView)
        {
            var workplace = workplaces.Find(x => x.View == buildingView);
            workplaces.Remove(workplace);
        }

        public void Tick()
        {
            foreach (var workplace in workplaces)
                Work(workplace);
        }

        private WorkplaceModel CreateModel(BuildingDefinition buildingDefinition)
        {
            var economyData = economyImporter.EconomyData[buildingDefinition];
            var definition = new WorkplaceDefinition()
            {
                Name = buildingDefinition.ToString(),

                ProcessedCommodity = economyData.ProcessedCommodity != null
                ? new CommodityModel(economyData.ProcessedCommodity.Value, economyData.ProcessedCommodityQuantity, 0)
                : null,

                ProcessingTime = economyData.ProcessingTime,
                MinimumWorkersCount = economyData.MinimumWorkersCount,
                MaxWorkersCount = economyData.MaxWorkersCount
            };

            return new WorkplaceModel(definition);
        }

        private void Work(WorkplacePresenter workplace)
        {
            storage = supplyModel.SupplyTargets[workplace.View];
            processedCommodity = workplace.Model.WorkplaceDefinition.ProcessedCommodity;

            if (storage == null || processedCommodity == null)
            {
                Debug.LogError($"Workplace {workplace.Model.WorkplaceDefinition} requires both a storage and a processed commodity.");
                return;
            }

            if (workplace.Model.CurrentWorkersCount < workplace.Model.WorkplaceDefinition.MinimumWorkersCount)
                return;

            if (storage.HasCommodities(processedCommodity.Name))
                ScheduleTransport(workplace);

            if (!storage.HasStorageRoom(processedCommodity.Name, processedCommodity.Quantity))
                return;

            var efficiency = Mathf.Clamp01((float)workplace.Model.CurrentWorkersCount / workplace.Model.WorkplaceDefinition.MaxWorkersCount);
            var progress = workplace.Model.ProcessingProgress;
            var progressDelta = Time.deltaTime / workplace.Model.WorkplaceDefinition.ProcessingTime * efficiency;
            workplace.Model.SetProcessingProgress(progress + progressDelta);

            if (workplace.Model.ProcessingProgress >= 1)
            {
                storage.AddCommodity(new CommodityModel
                {
                    Name = processedCommodity.Name,
                    Quantity = processedCommodity.Quantity
                });

                workplace.Model.SetProcessingProgress(0);

                if (storage.HasCommodities(workplace.Model.WorkplaceDefinition.ProcessedCommodity.Name))
                    ScheduleTransport(workplace);
            }
        }

        private void ScheduleTransport(WorkplacePresenter workplace)
        {
            if (!workplace.Model.IsCarrierAvailable)
                return;

            var result = BuildCarrierTasks(out Queue<CarrierTask> tasks, workplace);
            if (result == false)
                return;

            workplace.Model.UseCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, () => workplace.Model.ReturnCarrier(), workplace.Model));
        }

        private bool BuildCarrierTasks(out Queue<CarrierTask> tasks, WorkplacePresenter workplace)
        {
            tasks = default;

            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(
                workplace.View.transform.position, processedCommodity.Name, processedCommodity.Quantity);

            if (targetWithFreeSpace == null)
                return false;

            var taskBuilder = new CarrierTaskBuilder()
                .AddTaskWithReservation(storage, targetWithFreeSpace, new CommodityModel()
                {
                    Name = processedCommodity.Name,
                    Quantity = processedCommodity.Quantity
                },
                ReservationType.Space)
                .AddTask(targetWithFreeSpace, storage);

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }

    public struct WorkplacePresenter
    {
        public WorkplaceModel Model { get; private set; }

        public BuildingView View { get; private set; }

        public WorkplacePresenter(WorkplaceModel model, BuildingView view)
        {
            Model = model;
            View = view;
        }
    }
}