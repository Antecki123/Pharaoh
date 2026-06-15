namespace Models.Economy
{
    public class CommodityModel
    {
        public CommodityName Name { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; } = int.MaxValue;

        public CommodityModel() { }

        public CommodityModel(CommodityName name, int quantity, int maxQuantity)
        {
            Name = name;
            Quantity = quantity;
            MaxQuantity = maxQuantity;
        }

        public static CommodityModel Clone(CommodityModel other)
        {
            return new CommodityModel()
            {
                Name = other.Name,
                Quantity = other.Quantity,
                MaxQuantity = other.MaxQuantity,
            };
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