using App.Signals;
using Models.Economy;
using Models.Work;
using System;
using UnityEngine;
using Zenject;

namespace Controllers.Work
{
    public class WorkplacesController : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly SupplyModel supplyModel;
        private readonly EmploymentRepository employmentRepository;
        private readonly WorkplaceRepository workplaceRepository;
        private readonly WorkplaceEconomyImporter economyImporter;

        private readonly RawResourceProducerWorkplace rawResourceProducer;
        private readonly MaterialProcessingWorkplace materialProcessing;
        private readonly DistributionPointWorkplace distributionPoint;
        private readonly FarmWorkplace farmWorkplace;
        private readonly StorageWorkplace storageWorkplace;

        public WorkplacesController(SignalBus signalBus, SupplyModel supplyModel, EmploymentRepository employmentRepository,
            WorkplaceRepository workplaceRepository, WorkplaceEconomyImporter economyImporter,
            RawResourceProducerWorkplace.Factory rawResourceProducerFactory,
            MaterialProcessingWorkplace.Factory materialProcessingFactory,
            DistributionPointWorkplace.Factory distributionPointFactory,
            FarmWorkplace.Factory farmWorkplaceFactory,
            StorageWorkplace.Factory storageWorkplaceFactory)
        {
            this.signalBus = signalBus;
            this.supplyModel = supplyModel;
            this.employmentRepository = employmentRepository;
            this.economyImporter = economyImporter;
            this.workplaceRepository = workplaceRepository;

            rawResourceProducer = rawResourceProducerFactory.Create();
            materialProcessing = materialProcessingFactory.Create();
            distributionPoint = distributionPointFactory.Create();
            farmWorkplace = farmWorkplaceFactory.Create();
            storageWorkplace = storageWorkplaceFactory.Create();
        }

        public void Initialize()
        {
            signalBus.Subscribe<WorkplaceSignals.RegisterWorkplace>(RegisterWorkplace);
            signalBus.Subscribe<WorkplaceSignals.UnregisterWorkplace>(UnregisterWorkplace);
            signalBus.Subscribe<WorkplaceSignals.RegisterSupplyTarget>(RegisterSupplyTarget);
            signalBus.Subscribe<WorkplaceSignals.UnregisterSupplyTarget>(UnregisterSupplyTarget);
        }

        public void Tick()
        {
            rawResourceProducer.Tick();
            materialProcessing.Tick();
            distributionPoint.Tick();
            //farmWorkplace.Tick();
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<WorkplaceSignals.RegisterWorkplace>(RegisterWorkplace);
            signalBus.Unsubscribe<WorkplaceSignals.UnregisterWorkplace>(UnregisterWorkplace);
            signalBus.Unsubscribe<WorkplaceSignals.RegisterSupplyTarget>(RegisterSupplyTarget);
            signalBus.Unsubscribe<WorkplaceSignals.UnregisterSupplyTarget>(UnregisterSupplyTarget);
        }

        private void RegisterWorkplace(WorkplaceSignals.RegisterWorkplace signal)
        {
            var buildingDefinition = signal.BuildingView.BuildingDefinition;
            var workplaceData = economyImporter.EconomyData[buildingDefinition];
            var workplace = workplaceData.WorkplaceType switch
            {
                WorkplaceType.RawResourceProducer => rawResourceProducer.RegisterWorkplace(signal.BuildingView),
                WorkplaceType.MaterialProcessing => materialProcessing.RegisterWorkplace(signal.BuildingView),
                WorkplaceType.DistributionPoint => distributionPoint.RegisterWorkplace(signal.BuildingView),
                //WorkplaceType.FarmWorkplace => farmWorkplace.RegisterWorkplace(signal.BuildingView),
                WorkplaceType.Storage => storageWorkplace.RegisterWorkplace(signal.BuildingView),
                _ => throw new ArgumentOutOfRangeException(nameof(workplaceData.WorkplaceType), workplaceData.WorkplaceType, null)
            };

            workplaceRepository.AddWorkplace(signal.BuildingView, workplace);

            var emplyerModel = new EmplyerModel(signal.BuildingView, workplaceData.MaxWorkersCount);
            employmentRepository.AddEmplyer(signal.BuildingView, emplyerModel);
        }

        private void UnregisterWorkplace(WorkplaceSignals.UnregisterWorkplace signal)
        {
            var workplaceType = economyImporter.EconomyData[signal.BuildingView.BuildingDefinition].WorkplaceType;
            switch (workplaceType)
            {
                case WorkplaceType.RawResourceProducer:
                    rawResourceProducer.UnregisterWorkplace(signal.BuildingView);
                    break;
                case WorkplaceType.MaterialProcessing:
                    materialProcessing.UnregisterWorkplace(signal.BuildingView);
                    break;
                case WorkplaceType.DistributionPoint:
                    distributionPoint.UnregisterWorkplace(signal.BuildingView);
                    break;
                case WorkplaceType.FarmWorkplace:
                    //farmWorkplace.UnregisterWorkplace(signal.BuildingView);
                    break;
                case WorkplaceType.Storage:
                    storageWorkplace.UnregisterWorkplace(signal.BuildingView);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(workplaceType), workplaceType, null);
            }

            workplaceRepository.RemoveWorkplace(signal.BuildingView);
            employmentRepository.RemoveEmplyer(signal.BuildingView);
        }

        private void RegisterSupplyTarget(WorkplaceSignals.RegisterSupplyTarget signal)
        {
            if (!economyImporter.StorageData.TryGetValue(signal.BuildingView.BuildingDefinition, out var storageData))
            {
                Debug.LogError($"Storage data not found for building definition: {signal.BuildingView.BuildingDefinition}");
                return;
            }

            var storage = new StorageModel(storageData);
            supplyModel.RegisterSupplyTarget(signal.BuildingView, storage);
        }

        private void UnregisterSupplyTarget(WorkplaceSignals.UnregisterSupplyTarget signal)
        {
            supplyModel.RemoveSupplyTarget(signal.BuildingView);
        }
    }

    public interface IWorkplace
    {
        public void AddWorker();

        public void RemoveWorker();

        public bool IsRunning { get; }
    }

    public enum WorkplaceType
    {
        RawResourceProducer,
        MaterialProcessing,
        DistributionPoint,
        FarmWorkplace,
        Storage
    }
}