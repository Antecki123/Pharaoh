using App.Helpers;
using System;
using System.Collections.Generic;

namespace Controllers.Construction
{
    public class ConstructionDataImporter
    {
        public IReadOnlyDictionary<BuildingDefinition, ConstructionData> ConstructionData => constructionData;

        private Dictionary<BuildingDefinition, ConstructionData> constructionData = new Dictionary<BuildingDefinition, ConstructionData>();

        private CSVReader reader;
        private string CONSTRUCTION_DATA_PATH = "Importers/ConstructionData";

        public ConstructionDataImporter()
        {
            reader = new CSVReader();
            reader.ReadFile(CONSTRUCTION_DATA_PATH);

            LoadData();
        }

        private void LoadData()
        {
            var index = 1;
            while (true)
            {
                var text = reader.GetSplitedLine(index);
                if (text == null || string.IsNullOrEmpty(text[0]))
                    break;

                var definition = Enum.TryParse(text[0], out BuildingDefinition buildingDefinition) ? buildingDefinition : default;
                var data = new ConstructionData()
                {
                    Width = int.TryParse(text[1], out int width) ? width : default,
                    Height = int.TryParse(text[2], out int height) ? height : default
                };

                constructionData.Add(definition, data);
                index++;
            }
        }
    }

    public struct ConstructionData
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}