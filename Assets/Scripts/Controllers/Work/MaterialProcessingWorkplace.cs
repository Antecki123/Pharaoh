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
    public class MaterialProcessingWorkplace
    {
        public class Factory : PlaceholderFactory<MaterialProcessingWorkplace> { }

        private readonly SignalBus signalBus;
        private readonly SupplyModel supplyModel;
        private readonly WorkplaceEconomyImporter economyImporter;

        private readonly List<WorkplacePresenter> workplaces = new();

        private StorageModel storage;
        private CommodityModel requiredCommodity;
        private CommodityModel processedCommodity;

        public MaterialProcessingWorkplace(SignalBus signalBus, SupplyModel supplyModel,
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

        public IWorkplace UnregisterWorkplace(BuildingView buildingView)
        {
            var workplace = workplaces.Find(x => x.View == buildingView);

            if (workplace.Model == null)
                return default;

            workplaces.Remove(workplace);
            return workplace.Model;
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

                RequiredCommodity = economyData.RequiredCommodity != null
                ? new CommodityModel(economyData.RequiredCommodity.Value, economyData.RequiredCommodityQuantity, 0)
                : null,

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
            requiredCommodity = workplace.Model.WorkplaceDefinition.RequiredCommodity;
            processedCommodity = workplace.Model.WorkplaceDefinition.ProcessedCommodity;

            if (storage == null || requiredCommodity == null || processedCommodity == null)
            {
                Debug.LogError($"Workplace {workplace.Model.WorkplaceDefinition} is missing one or more required references: " +
                    $"storage, required commodity, or processed commodity.");
                return;
            }

            if (storage.HasCommodities(processedCommodity.Name) || !storage.HasCommodities(requiredCommodity.Name))
                ScheduleTransport(workplace);

            if (workplace.Model.CurrentWorkersCount < workplace.Model.WorkplaceDefinition.MinimumWorkersCount)
                return;

            if (!storage.HasCommodities(requiredCommodity.Name) 
                || !storage.HasStorageRoom(processedCommodity.Name, processedCommodity.Quantity))
                return;

            var efficiency = Mathf.Clamp01((float)workplace.Model.CurrentWorkersCount / workplace.Model.WorkplaceDefinition.MaxWorkersCount);
            var progress = workplace.Model.ProcessingProgress;
            var progressDelta = Time.deltaTime / workplace.Model.WorkplaceDefinition.ProcessingTime * efficiency;
            workplace.Model.SetProcessingProgress(progress + progressDelta);

            if (workplace.Model.ProcessingProgress >= 1)
            {
                storage.RemoveCommodity(new CommodityModel
                {
                    Name = requiredCommodity.Name,
                    Quantity = requiredCommodity.Quantity
                });

                storage.AddCommodity(new CommodityModel
                {
                    Name = processedCommodity.Name,
                    Quantity = processedCommodity.Quantity
                });

                workplace.Model.SetProcessingProgress(0);

                if (storage.HasCommodities(processedCommodity.Name)
                    || !storage.HasCommodities(requiredCommodity.Name))
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

            void OnCarrierReturn() => workplace.Model.ReturnCarrier();
            signalBus.Fire(new WorkplaceSignals.SpawnCarrier(tasks, OnCarrierReturn, workplace.Model));
        }

        private bool BuildCarrierTasks(out Queue<CarrierTask> tasks, WorkplacePresenter workplace)
        {
            tasks = default;

            var targetWithFreeSpace = supplyModel.GetClosestStorageWithFreeSpace(
                workplace.View.transform.position, processedCommodity.Name, processedCommodity.Quantity);

            var targetWithCommodity = supplyModel.GetClosestStorageWithCommodity(
               workplace.View.transform.position, requiredCommodity.Name, requiredCommodity.Quantity);

            if (targetWithFreeSpace == null && targetWithCommodity == null)
                return false;

            var taskBuilder = new CarrierTaskBuilder();
            if (storage.HasCommodities(processedCommodity.Name) && storage.HasCommodities(requiredCommodity.Name))
            {
                if (targetWithFreeSpace != null)
                    taskBuilder
                    .AddTaskWithReservation(storage, targetWithFreeSpace, processedCommodity, ReservationType.Space)
                    .AddTask(targetWithFreeSpace, storage);
                else
                    return false;
            }

            else if (!storage.HasCommodities(processedCommodity.Name) && !storage.HasCommodities(requiredCommodity.Name))
            {
                if (targetWithCommodity != null)
                    taskBuilder
                        .AddTask(storage, targetWithCommodity)
                        .AddTaskWithReservation(targetWithCommodity, storage, requiredCommodity, ReservationType.Commodity);
                else
                    return false;
            }

            else if (storage.HasCommodities(processedCommodity.Name) && !storage.HasCommodities(requiredCommodity.Name))
            {
                if (targetWithFreeSpace != null && targetWithCommodity != null)
                    taskBuilder
                    .AddTaskWithReservation(storage, targetWithFreeSpace, processedCommodity, ReservationType.Space)
                    .AddTask(targetWithFreeSpace, targetWithCommodity)
                    .AddTaskWithReservation(targetWithCommodity, storage, requiredCommodity, ReservationType.Commodity);
                else
                    return false;
            }

            tasks = new Queue<CarrierTask>(taskBuilder.Tasks);
            return true;
        }
    }
}