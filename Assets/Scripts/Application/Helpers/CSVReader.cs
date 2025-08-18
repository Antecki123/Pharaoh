using System;
using UnityEngine;

namespace App.Helpers
{
    public class CSVReader
    {
        private string[] splitDataset;
        private char delimiter = ';';

        public void ReadFile(string path)
        {
            var dataset = Resources.Load<TextAsset>(path);
            splitDataset = dataset.text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public string[] GetSplitedLine(int index)
        {
            if (splitDataset.Length > index)
                return splitDataset[index].Split(delimiter);
            else
                return null;
        }
    }
}