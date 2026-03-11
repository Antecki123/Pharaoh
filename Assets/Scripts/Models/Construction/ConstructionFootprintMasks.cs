using Controllers.Construction;
using System.Collections.Generic;

namespace Models.Construction
{
    public class ConstructionFootprintMasks
    {
        public static IReadOnlyDictionary<BuildingDefinition, int[,]> ConstructionFootprintMask => constructionFootprintMask;

        private static Dictionary<BuildingDefinition, int[,]> constructionFootprintMask = new Dictionary<BuildingDefinition, int[,]>()
        {
            {
                BuildingDefinition.Road, new int[,]
                {
                    { 1 }
                }
            },

            {
                BuildingDefinition.Cottage, new int[,]
                {
                    { 1, 1 },
                    { 1, 1 }
                }
            },

            {
                BuildingDefinition.House, new int[,]
                {
                    { 1, 1 },
                    { 1, 1 }
                }
            },

            {
                BuildingDefinition.Residence, new int[,]
                {
                    { 1, 1, 1 },
                    { 1, 1, 1 },
                    { 1, 1, 1 }
                }
            },

            {
                BuildingDefinition.Bazaar, new int[,]
                {
                    { 1, 1, 1, 1, 1, 1 },
                    { 1, 1, 1, 1, 1, 1 },
                    { 1, 1, 1, 1, 1, 1 }
                }
            },

            {
                BuildingDefinition.Warehouse, new int[,]
                {
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 1 }
                }
            },

            {
                BuildingDefinition.Bakery, new int[,]
                {
                    { 1, 1 },
                    { 1, 1 }
                }
            },

            {
                BuildingDefinition.Windmill, new int[,]
                {
                    { 1, 1, 1 },
                    { 1, 1, 1 },
                    { 1, 1, 1 }
                }
            },

            {
                BuildingDefinition.Granary, new int[,]
                {
                    { 1, 1, 1, 1, 1, 1 },
                    { 1, 1, 1, 1, 1, 1 },
                    { 1, 1, 1, 1, 1, 1 },
                    { 1, 1, 1, 1, 1, 1 }
                }
            },

            {
                BuildingDefinition.WheatFarm, new int[,]
                {
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 0 },
                }
            },

            {
                BuildingDefinition.LinenFarm, new int[,]
                {
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 0 },
                }
            },

            {
                BuildingDefinition.Pasture, new int[,]
                {
                    { 1, 1, 1, 1, 1 },
                    { 1, 1, 1, 1, 1 },
                    { 1, 1, 1, 1, 0 },
                    { 1, 1, 1, 1, 0 },
                }
            },

            {
                BuildingDefinition.Well, new int[,]
                {
                    { 1 }
                }
            },

            {
                BuildingDefinition.Brewery, new int[,]
                {
                    { 1, 1, 1 },
                    { 1, 1, 1 },
                    { 1, 1, 1 }
                }
            },

            {
                BuildingDefinition.WeavingMill, new int[,]
                {
                    { 1, 1 },
                    { 1, 1 }
                }
            },
        };
    }
}