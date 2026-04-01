using Models.Economy;
using Zenject;

namespace Models.Gameplay
{
    public class GatherGoldObjective : IObjective
    {
        public class Factory : PlaceholderFactory<GatherGoldObjectiveDefinition, GatherGoldObjective> { }

        public string Name { get; private set; }

        public bool IsFulfilled { get; private set; }

        public string ProgressDisplay => $"{economyModel.Currency}/ {currencyAmount}";

        private int currencyAmount;
        private EconomyModel economyModel;

        public GatherGoldObjective(GatherGoldObjectiveDefinition definition, EconomyModel economyModel)
        {
            Name = definition.Name;
            currencyAmount = definition.CurrencyAmount;

            this.economyModel = economyModel;
        }

        public void Process()
        {
            IsFulfilled = economyModel.Currency >= currencyAmount;
        }
    }

    public class GatherGoldObjectiveDefinition : Objective
    {
        public override string Name => "GatherGoldObjective";

        public int CurrencyAmount;
    }
}