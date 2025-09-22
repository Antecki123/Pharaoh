namespace Models.Economy
{
    public class EconomyModel
    {
        public int Settlers { get; set; }
        public int Gold { get; set; }
        public CommodityModel Food { get; set; }
        public CommodityModel Wood { get; set; }
        public CommodityModel Stone { get; set; }
    }
}