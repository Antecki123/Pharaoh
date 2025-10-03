using UnityEngine;

namespace Models.Economy
{
    public class CommodityModel
    {
        public CommodityName Name { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; } = int.MaxValue;

        public Sprite Sprite { get; set; }
        public GameObject Prefab { get; set; }
    }

    public static class CommodityExtensions
    {
        private static readonly CommodityName[] Categories =
        {
            CommodityName.Food
        };

        public static CommodityName GetCategory(this CommodityName commodity)
        {
            foreach (var category in Categories)
            {
                if (category.HasFlag(commodity))
                    return category;
            }

            return commodity;
        }
    }

    [System.Flags]
    public enum CommodityName
    {
        None = 0,
        Wheat = 1 << 0,
        Flour = 1 << 1,
        Bread = 1 << 2,
        Linen = 1 << 3,
        Beer = 1 << 4,
        Clothes = 1 << 5,
        Meat = 1 << 6,

        Food = Bread | Meat
    }
}