using System.Collections.Generic;
using UnityEngine;

namespace Models.Environment
{
    public class IrrigationModel
    {
        public IReadOnlyCollection<IrrigationData> IrrigationTiles => irrigationTiles;

        private HashSet<IrrigationData> irrigationTiles = new HashSet<IrrigationData>();

        public void AddIrrigationSource(Vector2Int tilePosition, int range)
        {

        }

        public void RemoveIrrigationSource(Vector2Int tilePosition)
        {

        }
    }

    public class IrrigationData
    {
        public Vector2Int Position { get; set; }

        public float Value { get; set; }
    }
}