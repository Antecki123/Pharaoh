using UnityEngine;

namespace Models.Economy
{
    public class CommodityModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; } = int.MaxValue;

        public Sprite Sprite { get; set; }
        public GameObject Model {  get; set; }
    }
}