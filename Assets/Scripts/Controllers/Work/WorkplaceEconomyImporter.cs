using App.Helpers;
using Controllers.Construction;
using Models.Economy;
using Models.Environment;
using System;
using System.Collections.Generic;

namespace Controllers.Work
{
    public class WorkplaceEconomyImporter
    {
        public IReadOnlyDictionary<BuildingDefinition, WorkplaceEconomyData> EconomyData => economyData;
        public IReadOnlyDictionary<BuildingDefinition, List<StorageEconomyData>> StorageData => storageData;
        public IReadOnlyDictionary<BuildingDefinition, InfluenceData> InfluenceData => influenceData;

        private readonly Dictionary<BuildingDefinition, WorkplaceEconomyData> economyData = new();
        private readonly Dictionary<BuildingDefinition, List<StorageEconomyData>> storageData = new();
        private readonly Dictionary<BuildingDefinition, InfluenceData> influenceData = new();

        private readonly CSVReader reader;
        private readonly string WORKPLACE_ECONOMY_PATH = "Importers/WorkplaceEconomy";
        private readonly string STORAGE_DATA_PATH = "Importers/StorageData";
        private readonly string INFLUENCE_DATA_PATH = "Importers/InfluenceData";

        public WorkplaceEconomyImporter()
        {
            reader = new CSVReader();

            reader.ReadFile(WORKPLACE_ECONOMY_PATH);
            LoadEconomyData();

            reader.ReadFile(STORAGE_DATA_PATH);
            LoadStorageData();

            //reader.ReadFile(INFLUENCE_DATA_PATH);
            //LoadInfluenceData();
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
                    WorkplaceType = Enum.TryParse(text[1], out WorkplaceType workplaceType) ? workplaceType : default,
                    RequiredCommodity = Enum.TryParse(text[2], out CommodityName commodityDefinition) ? commodityDefinition : null,
                    RequiredCommodityQuantity = int.TryParse(text[3], out int requiredCommodityQuantity) ? requiredCommodityQuantity : default,
                    ProcessedCommodity = Enum.TryParse(text[4], out CommodityName processedDefinition) ? processedDefinition : null,
                    ProcessedCommodityQuantity = int.TryParse(text[5], out int processedCommodityQuantity) ? processedCommodityQuantity : default,
                    ProcessingTime = float.TryParse(text[6], out float processingTime) ? processingTime : default,
                    MinimumWorkersCount = int.TryParse(text[7], out int minimumWorkersCount) ? minimumWorkersCount : default,
                    MaxWorkersCount = int.TryParse(text[8], out int maxWorkersCount) ? maxWorkersCount : default,
                    CarriersCount = int.TryParse(text[9], out int carriersCount) ? carriersCount : default
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
                    MaxQuantity = int.TryParse(text[3], out int maxQuantity) ? maxQuantity : default,
                    CommodityVisibility = Enum.TryParse(text[4], out CommodityVisibility commodityVisibility) ? commodityVisibility : default
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

        private void LoadInfluenceData()
        {
            var index = 1;
            while (true)
            {
                var text = reader.GetSplitedLine(index);
                if (text == null || string.IsNullOrEmpty(text[0]))
                    break;

                var definition = Enum.TryParse(text[0], out BuildingDefinition buildingDefinition) ? buildingDefinition : default;
                var data = new InfluenceData()
                {
                    InfluenceType = Enum.TryParse(text[1], out InfluenceType influenceType) ? influenceType : default,
                    InfluenceRange = float.TryParse(text[2], out float range) ? range : default,
                    InfluenceValue = float.TryParse(text[3], out float value) ? value : default
                };

                influenceData.Add(definition, data);
                index++;
            }
        }
    }

    public class WorkplaceEconomyData
    {
        public WorkplaceType WorkplaceType { get; set; }

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

        public CommodityVisibility CommodityVisibility { get; set; }
    }

    public class InfluenceData
    {
        public InfluenceType InfluenceType { get; set; }

        public float InfluenceRange { get; set; }

        public float InfluenceValue { get; set; }
    }
}