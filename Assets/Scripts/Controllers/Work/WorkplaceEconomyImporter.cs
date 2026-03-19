using App.Helpers;
using Controllers.Construction;
using Models.Economy;
using System;
using System.Collections.Generic;

namespace Controllers.Work
{
    public class WorkplaceEconomyImporter
    {
        public IReadOnlyDictionary<BuildingDefinition, WorkplaceEconomyData> EconomyData => economyData;

        public IReadOnlyDictionary<BuildingDefinition, List<StorageEconomyData>> StorageData => storageData;

        private Dictionary<BuildingDefinition, WorkplaceEconomyData> economyData = new Dictionary<BuildingDefinition, WorkplaceEconomyData>();
        private Dictionary<BuildingDefinition, List<StorageEconomyData>> storageData = new Dictionary<BuildingDefinition, List<StorageEconomyData>>();

        private CSVReader reader;
        private readonly string WORKPLACE_ECONOMY_PATH = "Importers/WorkplaceEconomy";
        private readonly string STORAGE_DATA_PATH = "Importers/StorageData";

        public WorkplaceEconomyImporter()
        {
            reader = new CSVReader();

            reader.ReadFile(WORKPLACE_ECONOMY_PATH);
            LoadEconomyData();

            reader.ReadFile(STORAGE_DATA_PATH);
            LoadStorageData();
        }

        private void LoadEconomyData()
        {
            var index = 1;
            while (true)
            {
                var text = reader.GetSplitedLine(index);
                if (text == null || string.IsNullOrEmpty(text[0]))
                    break;

                var definition = Enum.TryParse(text[0], out BuildingDefinition buildingDefinition) ? buildingDefinition : default;
                var data = new WorkplaceEconomyData()
                {
                    RequiredCommodity = Enum.TryParse(text[1], out CommodityName commodityDefinition) ? commodityDefinition : null,
                    RequiredCommodityQuantity = int.TryParse(text[2], out int requiredCommodityQuantity) ? requiredCommodityQuantity : default,
                    ProcessedCommodity = Enum.TryParse(text[3], out CommodityName processedDefinition) ? processedDefinition : null,
                    ProcessedCommodityQuantity = int.TryParse(text[4], out int processedCommodityQuantity) ? processedCommodityQuantity : default,
                    ProcessingTime = float.TryParse(text[5], out float processingTime) ? processingTime : default,
                    MinimumWorkersCount = int.TryParse(text[6], out int minimumWorkersCount) ? minimumWorkersCount : default,
                    MaxWorkersCount = int.TryParse(text[7], out int maxWorkersCount) ? maxWorkersCount : default,
                    CarriersCount = int.TryParse(text[8], out int carriersCount) ? carriersCount : default
                };

                economyData.Add(definition, data);
                index++;
            }
        }

        private void LoadStorageData()
        {
            var index = 1;
            while (true)
            {
                var text = reader.GetSplitedLine(index);
                if (text == null || string.IsNullOrEmpty(text[0]))
                    break;

                var definition = Enum.TryParse(text[0], out BuildingDefinition buildingDefinition) ? buildingDefinition : default;
                var data = new StorageEconomyData()
                {
                    Name = Enum.TryParse(text[1], out CommodityName commodityDefinition) ? commodityDefinition : default,
                    Quantity = int.TryParse(text[2], out int quantity) ? quantity : default,
                    MaxQuantity = int.TryParse(text[3], out int maxQuantity) ? maxQuantity : default
                };

                if (!storageData.TryGetValue(definition, out var list))
                {
                    list = new List<StorageEconomyData>();
                    storageData.Add(definition, list);
                }

                list.Add(data);

                index++;
            }
        }
    }

    public class WorkplaceEconomyData
    {
        public CommodityName? RequiredCommodity { get; set; }

        public int RequiredCommodityQuantity { get; set; }

        public CommodityName? ProcessedCommodity { get; set; }

        public int ProcessedCommodityQuantity { get; set; }

        public float ProcessingTime { get; set; }

        public int MinimumWorkersCount { get; set; }

        public int MaxWorkersCount { get; set; }

        public int CarriersCount { get; set; }
    }

    public class StorageEconomyData
    {
        public CommodityName Name { get; set; }

        public int Quantity { get; set; }

        public int MaxQuantity { get; set; }
    }
}