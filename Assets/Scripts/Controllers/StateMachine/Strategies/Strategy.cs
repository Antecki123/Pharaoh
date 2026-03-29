using System;
using Views.Settler;
using Zenject;

namespace Controllers.Ai.Strategy
{
    public abstract class Strategy
    {
        protected AiBrain aiBrain;

        public void Tick()
        {
            aiBrain.UpdateAction();
        }

        protected void AddTransition(IState to, IState from, Func<bool> condition)
            => aiBrain.AddTransition(to, from, condition);

        protected void AddAnyTransition(IState to, Func<bool> condition)
            => aiBrain.AddAnyTransition(to, condition);
    }

    public class StrategyFactory
    {
        private SettlerStrategy.Factory settlerStrategy;
        private ImmigrantStrategy.Factory immigrantStrategy;

        [Inject]
        public void Constructor(SettlerStrategy.Factory settlerStrategy, ImmigrantStrategy.Factory immigrantStrategy)
        {
            this.settlerStrategy = settlerStrategy;
            this.immigrantStrategy = immigrantStrategy;
        }

        public Strategy GetStrategy(SettlerView settlerView, StrategyDefinition strategyDefinition)
        {
            return strategyDefinition switch
            {
                StrategyDefinition.Immigrant => immigrantStrategy.Create(settlerView),
                StrategyDefinition.Settler => settlerStrategy.Create(settlerView),
                _ => throw new ArgumentException($"Unknown strategy type: {strategyDefinition}")
            };
        }
    }

    public enum StrategyDefinition
    {
        Immigrant,
        Settler
    }
}