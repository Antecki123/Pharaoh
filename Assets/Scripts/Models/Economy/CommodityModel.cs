using UnityEngine;

namespace Models.Economy
{
    public class CommodityModel
    {
        public CommodityName Name { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; } = int.MaxValue;

        public Sprite Sprite { get; set; }
        public GameObject Model {  get; set; }
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