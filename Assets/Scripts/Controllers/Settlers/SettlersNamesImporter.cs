using App.Helpers;
using System.Collections.Generic;

namespace Controllers.Settler
{
    public class SettlersNamesImporter
    {
        public List<string> MaleNames { get; private set; } = new List<string>();
        public List<string> FemaleNames { get; private set; } = new List<string>();

        private CSVReader reader;
        private string SETTLERS_NAMES_PATH = "Importers/SettlersNames";

        public SettlersNamesImporter()
        {
            reader = new CSVReader();
            reader.ReadFile(SETTLERS_NAMES_PATH);

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

                MaleNames.Add(text[0]);
                FemaleNames.Add(text[1]);

                index++;
            }
        }
    }
}