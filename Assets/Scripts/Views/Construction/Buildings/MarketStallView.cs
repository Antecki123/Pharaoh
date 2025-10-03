using Models.Economy;
using System.Collections.Generic;
using UnityEngine;

namespace Views.Construction
{
    [SelectionBase]
    public class MarketStallView : MonoBehaviour
    {
        private MarketStallModel marketStallModel;

        public void Init(MarketStallModel marketStallModel)
        {
            this.marketStallModel = marketStallModel;
        }

        public void CreateMarketStall()
        {

        }
    }

    public class MarketStallModel
    {
        public CommodityModel Commodity { get; set; }

        public bool IsAvailable { get; set; }

        public IReadOnlyList<object> Workers => workers;

        private List<object> workers = new List<object>();

        public MarketStallModel(CommodityModel commodity)
        {
            Commodity = commodity;
        }
    }
}
