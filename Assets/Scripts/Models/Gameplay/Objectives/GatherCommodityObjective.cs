using Models.Economy;
using System;
using Zenject;

namespace Models.Gameplay
{
    public class GatherCommodityObjective : IObjective
    {
        public class Factory : PlaceholderFactory<GatherCommodityObjectiveDefinition, GatherCommodityObjective> { }

        public string Name { get; private set; }

        public bool IsFulfilled { get; private set; }

        public string ProgressDisplay => $"{0}/{commodityCount}";

        private CommodityName commodityName;
        private int commodityCount;

        public GatherCommodityObjective(GatherCommodityObjectiveDefinition definition)
        {
            Name = definition.Name;
            commodityName = definition.CommodityName;
            commodityCount = definition.CommodityCount;
        }

        public void Process()
        {
            IsFulfilled = false;
        }
    }

    [Serializable]
    public class GatherCommodityObjectiveDefinition : Objective
    {
        public override string Name => "GatherCommodityObjective";

        public CommodityName CommodityName;

        public int CommodityCount;
    }
}