using Models.Economy;
using System;
using Zenject;

namespace Models.Gameplay
{
    public class ReachPopulationObjective : IObjective
    {
        public class Factory : PlaceholderFactory<ReachPopulationObjectiveDefinition, ReachPopulationObjective> { }

        public string Name { get; private set; }

        public bool IsFulfilled { get; private set; }

        public string ProgressDisplay => $"{economyModel.Settlers}/{populationCount}";

        private int populationCount;
        private EconomyModel economyModel;

        public ReachPopulationObjective(ReachPopulationObjectiveDefinition definition, EconomyModel economyModel)
        {
            Name = definition.Name;
            populationCount = definition.PopulationCount;

            this.economyModel = economyModel;
        }

        public void Process()
        {
            IsFulfilled = economyModel.Settlers >= populationCount;
        }
    }

    [Serializable]
    public class ReachPopulationObjectiveDefinition : Objective
    {
        public override string Name => "ReachPopulationObjective";

        public int PopulationCount;
    }
}