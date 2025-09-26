using App.Helpers;
using System.Collections.Generic;

namespace Controllers.Settler
{
    public class SettlersNamesImporter
    {
        public IReadOnlyList<string> MaleNames => maleNames;

        public IReadOnlyList<string> FemaleNames => femaleNames;

        private List<string> maleNames = new List<string>();
        private List<string> femaleNames = new List<string>();

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

                maleNames.Add(text[0]);
                femaleNames.Add(text[1]);

                index++;
            }
        }
    }
}