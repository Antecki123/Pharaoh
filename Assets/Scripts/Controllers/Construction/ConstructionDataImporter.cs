using App.Helpers;
using System;
using System.Collections.Generic;

namespace Controllers.Construction
{
    public class ConstructionDataImporter
    {
        public Dictionary<BuildingDefinition, ConstructionData> ConstructionData { get; } = new Dictionary<BuildingDefinition, ConstructionData>();

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
                    BuildingOffset = float.TryParse(text[1], out float buildingOffset) ? buildingOffset : default,
                    SnapDistance = float.TryParse(text[2], out float snapDistance) ? snapDistance : default
                };

                ConstructionData.Add(definition, data);
                index++;
            }
        }
    }

    public struct ConstructionData
    {
        public float BuildingOffset { get; set; }
        public float SnapDistance { get; set; }
    }
}